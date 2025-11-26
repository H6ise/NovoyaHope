using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using NovoyaHope.Data;
using NovoyaHope.Models;
using NovoyaHope.Models.ViewModels;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace NovoyaHope.Controllers
{
    [Authorize] // Доступ только для авторизованных пользователей
    public class SurveyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SurveyController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

        // --- СОХРАНЕНИЕ / ОБНОВЛЕНИЕ ЧЕРЕЗ AJAX ---

        // POST /api/surveys/save (используется AJAX из constructor.js)
        [HttpPost]
        [Route("api/surveys/save")]
        public async Task<IActionResult> SaveSurvey([FromBody] SaveSurveyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Здесь должна быть сложная логика CRUD для обновления Survey, Question и AnswerOption
            // (обработка model.Id == null для добавления, model.Id != null для обновления)

            await _context.SaveChangesAsync();
            return Ok(new { Success = true, SurveyId = model.Id, Message = "Сохранено" });
        }

        // --- ПУБЛИКАЦИЯ ---

        // POST /survey/publish/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            var survey = await _context.Surveys.FindAsync(id);
            var userId = _userManager.GetUserId(User);

            if (survey == null || survey.CreatorId != userId)
            {
                return Forbid();
            }

            if (!await _context.Questions.AnyAsync(q => q.SurveyId == id))
            {
                return BadRequest(new { message = "Нельзя опубликовать пустой опрос. Добавьте хотя бы один вопрос." });
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