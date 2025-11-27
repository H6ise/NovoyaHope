using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using NovoyaHope.Data;
using NovoyaHope.Models;
using NovoyaHope.Models.ViewModels;
using NovoyaHope.Services;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace NovoyaHope.Controllers
{
    [Authorize] // Доступ только для авторизованных пользователей
    public class SurveyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISurveyService _surveyService;

        public SurveyController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ISurveyService surveyService)
        {
            _context = context;
            _userManager = userManager;
            _surveyService = surveyService;
        }

        // --- СПИСОК МОИХ ОПРОСОВ ---

        // GET /survey/index
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var mySurveys = await _context.Surveys
                .Where(s => s.CreatorId == userId)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();

            // Маппинг mySurveys в ViewModel для отображения карточек
            return View(mySurveys);
        }

        // --- КОНСТРУКТОР (СОЗДАНИЕ/РЕДАКТИРОВАНИЕ) ---

        // GET /survey/edit/1 (или без ID для нового)
        public async Task<IActionResult> Edit(int? id)
        {
            // 1. Если id == null, создаем пустой опрос и перенаправляем на него.
            if (id == null)
            {
                var newSurvey = new Survey
                {
                    Title = "Новая форма",
                    Description = string.Empty,
                    CreatorId = _userManager.GetUserId(User),
                    CreatedDate = DateTime.UtcNow,
                    IsPublished = false
                };
                _context.Surveys.Add(newSurvey);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Edit), new { id = newSurvey.Id });
            }

            // 2. Загрузка существующего опроса и проверка прав
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (survey == null || survey.CreatorId != _userManager.GetUserId(User))
            {
                return NotFound();
            }

            // Здесь необходим маппинг Survey -> ViewModel для конструктора
            var viewModel = new NovoyaHope.Models.ViewModels.SurveyConstructorViewModel
            {
                Survey = survey,
                Questions = survey.Questions?.OrderBy(q => q.Order).ToList() ?? new System.Collections.Generic.List<NovoyaHope.Models.Question>()
            };

            return View("Constructor", viewModel);
        }

        // --- СОХРАНЕНИЕ / ОБНОВЛЕНИЕ ---

        // POST /survey/save (обработка обычной формы)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(SaveSurveyViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                // Если есть ошибки валидации, возвращаем обратно на страницу редактирования
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }

            try
            {
                var surveyId = await _surveyService.SaveSurveyAsync(model, userId);
                TempData["SuccessMessage"] = "Опросник успешно сохранен!";
                return RedirectToAction(nameof(Edit), new { id = surveyId });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при сохранении: {ex.Message}";
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
        }

        // POST /api/surveys/save (используется AJAX из constructor.js)
        [HttpPost]
        [Route("api/surveys/save")]
        public async Task<IActionResult> SaveSurvey([FromBody] SaveSurveyViewModel model)
        {
            // Логируем входящий запрос для отладки
            var logger = HttpContext.RequestServices.GetService<ILogger<SurveyController>>();
            logger?.LogInformation("SaveSurvey called. Model is null: {IsNull}", model == null);
            
            // Проверка модели
            if (model == null)
            {
                logger?.LogWarning("Model is null. Content-Type: {ContentType}, ContentLength: {Length}", 
                    Request.ContentType, Request.ContentLength);
                
                return BadRequest(new { success = false, message = "Данные не получены. Проверьте формат отправляемых данных." });
            }
            
            logger?.LogInformation("Model received. Id: {Id}, Title: {Title}, Questions count: {Count}", 
                model.Id, model.Title, model.Questions?.Count ?? 0);

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Success = false, Message = "Пользователь не авторизован." });
            }

            // Проверка обязательных полей вручную для более понятных сообщений
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                return BadRequest(new { Success = false, Message = "Укажите заголовок опроса." });
            }

            // Инициализируем Questions, если null
            if (model.Questions == null)
            {
                model.Questions = new List<SaveQuestionViewModel>();
            }

            if (!ModelState.IsValid)
            {
                // Собираем все ошибки валидации в понятный формат
                var errors = new List<string>();
                foreach (var error in ModelState)
                {
                    if (error.Value.Errors.Count > 0)
                    {
                        foreach (var errorMessage in error.Value.Errors)
                        {
                            errors.Add($"{error.Key}: {errorMessage.ErrorMessage}");
                        }
                    }
                }
                return BadRequest(new { Success = false, Message = string.Join("; ", errors), Errors = ModelState });
            }

            try
            {
                // Используем SurveyService для сохранения опроса с вопросами и вариантами ответов
                var surveyId = await _surveyService.SaveSurveyAsync(model, userId);
                return Ok(new { Success = true, SurveyId = surveyId, Message = "Сохранено" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (System.Exception ex)
            {
                // Логируем полную ошибку для отладки
                logger?.LogError(ex, "Ошибка при сохранении опроса. SurveyId: {SurveyId}, UserId: {UserId}", model.Id, userId);
                
                return BadRequest(new { Success = false, Message = $"Ошибка при сохранении: {ex.Message}", Exception = ex.GetType().Name });
            }
        }

        // --- ПУБЛИКАЦИЯ ---

        // POST /survey/publish/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            // Проверка anti-forgery token выполняется автоматически через [ValidateAntiForgeryToken]
            // Если токен невалиден, будет возвращена ошибка 400 до выполнения этого кода
            
            var survey = await _context.Surveys.FindAsync(id);
            var userId = _userManager.GetUserId(User);

            if (survey == null)
            {
                return BadRequest(new { success = false, message = "Опрос не найден." });
            }

            if (survey.CreatorId != userId)
            {
                return Forbid();
            }

            if (!await _context.Questions.AnyAsync(q => q.SurveyId == id))
            {
                return BadRequest(new { success = false, message = "Нельзя опубликовать пустой опрос. Добавьте хотя бы один вопрос." });
            }

            if (survey.IsPublished)
            {
                return BadRequest(new { success = false, message = "Опрос уже опубликован." });
            }

            survey.IsPublished = true;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Опросник успешно опубликован!", publicUrl = Url.Action("ViewSurvey", "Public", new { id = id }, Request.Scheme) });
        }

        // GET /survey/results/1
        public async Task<IActionResult> Results(int id)
        {
            var userId = _userManager.GetUserId(User);
            var survey = await _context.Surveys
                .Include(s => s.Questions.OrderBy(q => q.Order))
                    .ThenInclude(q => q.AnswerOptions.OrderBy(o => o.Order))
                .Include(s => s.Responses)
                    .ThenInclude(r => r.UserAnswers)
                        .ThenInclude(ua => ua.SelectedOption)
                .FirstOrDefaultAsync(s => s.Id == id && s.CreatorId == userId);

            if (survey == null)
            {
                return NotFound();
            }

            var viewModel = new SurveyResultsViewModel
            {
                SurveyId = survey.Id,
                SurveyTitle = survey.Title,
                TotalResponses = survey.Responses?.Count ?? 0,
                CreationDate = survey.CreatedDate,
                QuestionResults = new List<QuestionResultViewModel>()
            };

            // Обработка результатов по каждому вопросу
            foreach (var question in survey.Questions ?? new List<Question>())
            {
                var questionResult = new QuestionResultViewModel
                {
                    QuestionId = question.Id,
                    Text = question.Text,
                    Type = question.Type,
                    OptionCounts = new Dictionary<int, int>(),
                    OptionTexts = new Dictionary<int, string>(),
                    TextAnswers = new List<string>(),
                    AverageScore = 0
                };

                // Получаем все ответы на этот вопрос
                var answersForQuestion = survey.Responses?
                    .SelectMany(r => r.UserAnswers ?? new List<UserAnswer>())
                    .Where(ua => ua.QuestionId == question.Id)
                    .ToList() ?? new List<UserAnswer>();

                if (question.Type == QuestionType.ShortText || question.Type == QuestionType.ParagraphText)
                {
                    // Текстовые ответы
                    questionResult.TextAnswers = answersForQuestion
                        .Where(a => !string.IsNullOrWhiteSpace(a.TextAnswer))
                        .Select(a => a.TextAnswer)
                        .ToList();
                }
                else if (question.Type == QuestionType.SingleChoice || question.Type == QuestionType.MultipleChoice)
                {
                    // Ответы с выбором вариантов
                    foreach (var option in question.AnswerOptions ?? new List<AnswerOption>())
                    {
                        questionResult.OptionTexts[option.Id] = option.Text;
                        questionResult.OptionCounts[option.Id] = answersForQuestion
                            .Count(a => a.SelectedOptionId == option.Id);
                    }
                }
                else if (question.Type == QuestionType.Scale)
                {
                    // Шкала - считаем среднее значение
                    var scaleAnswers = answersForQuestion
                        .Where(a => a.SelectedOptionId.HasValue)
                        .Select(a => a.SelectedOption?.Order ?? 0)
                        .Where(order => order > 0)
                        .ToList();

                    if (scaleAnswers.Any())
                    {
                        questionResult.AverageScore = scaleAnswers.Average();
                    }

                    // Также показываем распределение по опциям
                    foreach (var option in question.AnswerOptions ?? new List<AnswerOption>())
                    {
                        questionResult.OptionTexts[option.Id] = option.Text;
                        questionResult.OptionCounts[option.Id] = answersForQuestion
                            .Count(a => a.SelectedOptionId == option.Id);
                    }
                }

                viewModel.QuestionResults.Add(questionResult);
            }

            return View("Results", viewModel);
        }

        // POST /survey/unpublish/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unpublish(int id)
        {
            var survey = await _context.Surveys.FindAsync(id);
            var userId = _userManager.GetUserId(User);

            if (survey == null || survey.CreatorId != userId)
            {
                return Forbid();
            }

            survey.IsPublished = false;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Опросник снят с публикации." });
        }
    }
}