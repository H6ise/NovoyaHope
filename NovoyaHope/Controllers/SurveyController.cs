using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using NovoyaHope.Data;
using NovoyaHope.Models;
using NovoyaHope.Models.ViewModels;
using NovoyaHope.Services;
using NovoyaHope.Helpers;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.IO;

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
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var newSurvey = new Survey
                {
                    Title = "Новая форма",
                    Description = string.Empty,
                    CreatorId = userId,
                    CreatedDate = DateTime.UtcNow,
                    IsPublished = false
                };
                _context.Surveys.Add(newSurvey);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Edit), new { id = newSurvey.Id });
            }

            // 2. Загрузка существующего опроса и проверка прав
            var survey = await _context.Surveys
                .Include(s => s.Questions!)
                    .ThenInclude(q => q.AnswerOptions!)
                .Include(s => s.Sections)
                .Include(s => s.Media)
                .Include(s => s.Responses!)
                    .ThenInclude(r => r.UserAnswers!)
                        .ThenInclude(ua => ua.SelectedOption)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (survey == null || survey.CreatorId != _userManager.GetUserId(User))
            {
                return NotFound();
            }

            // Подготовка данных ответов для вкладки "Ответы"
            SurveyResultsViewModel? resultsData = null;
            if (survey.Responses != null && survey.Responses.Any())
            {
                resultsData = new SurveyResultsViewModel
                {
                    SurveyId = survey.Id,
                    SurveyTitle = survey.Title,
                    TotalResponses = survey.Responses.Count,
                    CreationDate = survey.CreatedDate,
                    QuestionResults = new List<QuestionResultViewModel>()
                };

                // Обработка результатов по каждому вопросу
                var questionsList = survey.Questions?.OrderBy(q => q.Order).ToList() ?? new List<Question>();
                foreach (var question in questionsList)
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
                    var answersForQuestion = survey.Responses
                        .SelectMany(r => r.UserAnswers ?? new List<UserAnswer>())
                        .Where(ua => ua.QuestionId == question.Id)
                        .ToList();

                    if (question.Type == QuestionType.ShortText || question.Type == QuestionType.ParagraphText)
                    {
                        questionResult.TextAnswers = answersForQuestion
                            .Where(a => !string.IsNullOrWhiteSpace(a.TextAnswer))
                            .Select(a => a.TextAnswer!)
                            .ToList();
                    }
                    else if (question.Type == QuestionType.SingleChoice || question.Type == QuestionType.MultipleChoice)
                    {
                        foreach (var option in question.AnswerOptions ?? new List<AnswerOption>())
                        {
                            questionResult.OptionTexts[option.Id] = option.Text;
                            questionResult.OptionCounts[option.Id] = answersForQuestion
                                .Count(a => a.SelectedOptionId == option.Id);
                        }
                    }
                    else if (question.Type == QuestionType.Scale)
                    {
                        var scaleAnswers = answersForQuestion
                            .Where(a => a.SelectedOptionId.HasValue)
                            .Select(a => a.SelectedOption?.Order ?? 0)
                            .Where(order => order > 0)
                            .ToList();

                        if (scaleAnswers.Any())
                        {
                            questionResult.AverageScore = scaleAnswers.Average();
                        }

                        foreach (var option in question.AnswerOptions ?? new List<AnswerOption>())
                        {
                            questionResult.OptionTexts[option.Id] = option.Text;
                            questionResult.OptionCounts[option.Id] = answersForQuestion
                                .Count(a => a.SelectedOptionId == option.Id);
                        }
                    }

                    resultsData.QuestionResults.Add(questionResult);
                }
            }

            // Здесь необходим маппинг Survey -> ViewModel для конструктора
            var orderedQuestions = survey.Questions?.OrderBy(q => q.Order).ToList() ?? new System.Collections.Generic.List<Question>();
            var orderedSections = survey.Sections?.OrderBy(s => s.Order).ToList() ?? new System.Collections.Generic.List<Section>();
            var orderedMedia = survey.Media?.OrderBy(m => m.Order).ToList() ?? new System.Collections.Generic.List<Media>();
            
            var viewModel = new SurveyConstructorViewModel
            {
                Survey = survey,
                Questions = orderedQuestions,
                Sections = orderedSections,
                Media = orderedMedia,
                ResultsData = resultsData
            };

            return View("Constructor", viewModel);
        }

        // --- ПРЕДПРОСМОТР ---

        // GET /survey/preview/1
        public async Task<IActionResult> Preview(int id)
        {
            var userId = _userManager.GetUserId(User);
            var survey = await _context.Surveys
                .Include(s => s.Questions!)
                    .ThenInclude(q => q.AnswerOptions!)
                .Include(s => s.Sections)
                .Include(s => s.Media)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (survey == null || survey.CreatorId != userId)
            {
                return NotFound();
            }

            // Создаем ViewModel для предпросмотра (похож на PassSurveyViewModel, но без возможности отправки)
            var viewModel = new PassSurveyViewModel
            {
                Id = survey.Id,
                Title = survey.Title,
                Description = survey.Description,
                IsAnonymous = survey.IsAnonymous,
                Questions = (survey.Questions?.OrderBy(q => q.Order).Select(q => new PassQuestionViewModel
                {
                    Id = q.Id,
                    Text = q.Text,
                    Type = q.Type,
                    IsRequired = q.IsRequired,
                    Options = q.AnswerOptions?.OrderBy(o => o.Order).Select(o => new PassAnswerOptionViewModel
                    {
                        Id = o.Id,
                        Text = o.Text,
                        Order = o.Order
                    }).ToList() ?? new List<PassAnswerOptionViewModel>()
                }).ToList() ?? new List<PassQuestionViewModel>())!,
                Sections = (survey.Sections?.OrderBy(s => s.Order).ToList() ?? new List<Section>())!,
                Media = (survey.Media?.OrderBy(m => m.Order).ToList() ?? new List<Media>())!,
                // Передаем настройки темы
                ThemeColor = survey.ThemeColor,
                BackgroundColor = survey.BackgroundColor,
                HeaderImagePath = survey.HeaderImagePath,
                HeaderFontFamily = survey.HeaderFontFamily,
                HeaderFontSize = survey.HeaderFontSize,
                QuestionFontFamily = survey.QuestionFontFamily,
                QuestionFontSize = survey.QuestionFontSize,
                TextFontFamily = survey.TextFontFamily,
                TextFontSize = survey.TextFontSize
            };

            return View("Preview", viewModel);
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

            // Обработка загруженных файлов для медиа-изображений
            if (Request.Form.Files != null && Request.Form.Files.Count > 0 && model.Media != null && model.Media.Count > 0)
            {
                var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var tempSurveyId = model.Id ?? 0;
                
                // Если это новый опрос, сначала сохраняем его, чтобы получить Id
                if (tempSurveyId == 0)
                {
                    tempSurveyId = await _surveyService.SaveSurveyAsync(model, userId);
                }

                // Обрабатываем загруженные файлы для медиа
                for (int i = 0; i < model.Media.Count; i++)
                {
                    var media = model.Media[i];
                    var fileKey = $"Media[{i}].File";
                    var file = Request.Form.Files[fileKey];
                    
                    if (file != null && file.Length > 0)
                    {
                        try
                        {
                            var imageUrl = await ImageHelper.SaveMediaImageAsync(file, tempSurveyId, webRootPath);
                            media.Url = imageUrl;
                        }
                        catch (ArgumentException ex)
                        {
                            ModelState.AddModelError($"Media[{i}].File", ex.Message);
                        }
                    }
                    // Валидация: для изображений должен быть либо URL, либо загруженный файл
                    else if (media.Type == MediaType.Image && string.IsNullOrWhiteSpace(media.Url))
                    {
                        ModelState.AddModelError($"Media[{i}].Url", "Для изображения необходимо указать URL или загрузить файл.");
                    }
                }
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
            catch (UnauthorizedAccessException)
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
                .Include(s => s.Questions!)
                    .ThenInclude(q => q.AnswerOptions!)
                .Include(s => s.Responses!)
                    .ThenInclude(r => r.UserAnswers!)
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
                        .Select(a => a.TextAnswer!)
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

        // --- УДАЛЕНИЕ ОПРОСА ---

        // POST /survey/delete/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions!)
                    .ThenInclude(q => q.AnswerOptions!)
                .Include(s => s.Responses!)
                    .ThenInclude(r => r.UserAnswers!)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            var userId = _userManager.GetUserId(User);

            if (survey == null)
            {
                return NotFound();
            }

            // Проверка прав: только создатель может удалить свой опрос
            if (survey.CreatorId != userId)
            {
                return Forbid();
            }

            try
            {
                // Удаление опроса (каскадное удаление настроено в ApplicationDbContext)
                // Questions, AnswerOptions, Responses и UserAnswers удалятся автоматически
                _context.Surveys.Remove(survey);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Опрос успешно удален.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices.GetService<ILogger<SurveyController>>();
                logger?.LogError(ex, "Ошибка при удалении опроса. SurveyId: {SurveyId}, UserId: {UserId}", id, userId);
                
                TempData["ErrorMessage"] = "Ошибка при удалении опроса. Попробуйте еще раз.";
                return RedirectToAction(nameof(Index));
            }
        }

        // --- ЭКСПОРТ РЕЗУЛЬТАТОВ ---

        // GET /survey/export/{id}
        public async Task<IActionResult> ExportResults(int id, string format = "csv")
        {
            var userId = _userManager.GetUserId(User);
            var survey = await _context.Surveys
                .Include(s => s.Questions!)
                    .ThenInclude(q => q.AnswerOptions!)
                .Include(s => s.Responses!)
                    .ThenInclude(r => r.User)
                .Include(s => s.Responses!)
                    .ThenInclude(r => r.UserAnswers!)
                        .ThenInclude(ua => ua.SelectedOption)
                .FirstOrDefaultAsync(s => s.Id == id && s.CreatorId == userId);

            if (survey == null)
            {
                return NotFound();
            }

            if (format.ToLower() == "csv")
            {
                return await ExportToCsv(survey);
            }

            return BadRequest("Неподдерживаемый формат экспорта. Используйте 'csv'.");
        }

        private async Task<IActionResult> ExportToCsv(Survey survey)
        {
            var responses = survey.Responses?.OrderBy(r => r.SubmissionDate).ToList() ?? new List<SurveyResponse>();
            var questions = survey.Questions?.OrderBy(q => q.Order).ToList() ?? new List<Question>();

            using (var memoryStream = new MemoryStream())
            using (var writer = new StreamWriter(memoryStream, System.Text.Encoding.UTF8))
            {
                // Заголовок CSV
                var headers = new List<string> { "ID ответа", "Дата отправки", "Email пользователя" };
                headers.AddRange(questions.Select(q => EscapeCsvField(q.Text)));
                writer.WriteLine(string.Join(",", headers));

                // Данные по каждому ответу
                foreach (var response in responses)
                {
                    var row = new List<string>
                    {
                        response.Id.ToString(),
                        response.SubmissionDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        survey.IsAnonymous || response.User == null ? "Анонимно" : (response.User.Email ?? "Не указан")
                    };

                    // Для каждого вопроса находим ответ
                    foreach (var question in questions)
                    {
                        var answers = response.UserAnswers?
                            .Where(ua => ua.QuestionId == question.Id)
                            .ToList() ?? new List<UserAnswer>();

                        string answerText = "";

                        if (answers.Any())
                        {
                            if (question.Type == QuestionType.ShortText || question.Type == QuestionType.ParagraphText)
                            {
                                // Для текстовых вопросов берем первый ответ
                                answerText = answers.First().TextAnswer ?? "";
                            }
                            else if (question.Type == QuestionType.MultipleChoice)
                            {
                                // Для множественного выбора объединяем все выбранные варианты
                                var selectedOptions = answers
                                    .Where(a => a.SelectedOptionId.HasValue)
                                    .Select(a => question.AnswerOptions?
                                        .FirstOrDefault(o => o.Id == a.SelectedOptionId!.Value)?.Text)
                                    .Where(text => !string.IsNullOrEmpty(text))
                                    .Select(text => text!)
                                    .ToList();
                                
                                answerText = string.Join("; ", selectedOptions);
                            }
                            else if (answers.First().SelectedOptionId.HasValue)
                            {
                                // Для одиночного выбора берем первый ответ
                                var firstAnswer = answers.First();
                                if (firstAnswer.SelectedOptionId.HasValue)
                                {
                                    var optionId = firstAnswer.SelectedOptionId.Value;
                                    var option = question.AnswerOptions?
                                        .FirstOrDefault(o => o.Id == optionId);
                                    answerText = option?.Text ?? "";
                                }
                            }
                        }

                        row.Add(EscapeCsvField(answerText));
                    }

                    writer.WriteLine(string.Join(",", row));
                }

                await writer.FlushAsync();
                memoryStream.Position = 0;

                var fileName = $"Результаты_{EscapeFileName(survey.Title)}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                return File(memoryStream.ToArray(), "text/csv; charset=utf-8", fileName);
            }
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            // Если поле содержит запятую, кавычки или перенос строки, заключаем в кавычки
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                // Экранируем кавычки удвоением
                field = field.Replace("\"", "\"\"");
                return $"\"{field}\"";
            }

            return field;
        }

        private string EscapeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "survey";

            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                fileName = fileName.Replace(c, '_');
            }

            return fileName;
        }

        // --- ШАБЛОНЫ ---

        // GET /survey/templates
        public async Task<IActionResult> Templates()
        {
            var templates = await _context.SurveyTemplates
                .Include(t => t.Questions.OrderBy(q => q.Order))
                    .ThenInclude(q => q.AnswerOptions.OrderBy(o => o.Order))
                .OrderByDescending(t => t.Id)
                .ToListAsync();

            return View(templates);
        }

        // POST /survey/use-template/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UseTemplate(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var template = await _context.SurveyTemplates
                .Include(t => t.Questions.OrderBy(q => q.Order))
                    .ThenInclude(q => q.AnswerOptions.OrderBy(o => o.Order))
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
            {
                return NotFound();
            }

            // Создаем новый опрос на основе шаблона
            var newSurvey = new Survey
            {
                Title = template.Title,
                Description = template.Description,
                Type = template.Type,
                CreatorId = userId,
                CreatedDate = DateTime.UtcNow,
                IsPublished = false,
                IsAnonymous = true
            };

            _context.Surveys.Add(newSurvey);
            await _context.SaveChangesAsync();

            // Копируем вопросы из шаблона
            foreach (var templateQuestion in template.Questions)
            {
                var question = new Question
                {
                    SurveyId = newSurvey.Id,
                    Text = templateQuestion.Text,
                    Type = templateQuestion.Type,
                    IsRequired = true,
                    Order = templateQuestion.Order
                };

                _context.Questions.Add(question);
                await _context.SaveChangesAsync();

                // Копируем варианты ответов
                foreach (var templateOption in templateQuestion.AnswerOptions)
                {
                    var option = new AnswerOption
                    {
                        QuestionId = question.Id,
                        Text = templateOption.Text,
                        Order = templateOption.Order
                    };

                    _context.AnswerOptions.Add(option);
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Опрос создан на основе шаблона!";
            return RedirectToAction(nameof(Edit), new { id = newSurvey.Id });
        }
    }
}