using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovoyaHope.Data;
using NovoyaHope.Models;
using NovoyaHope.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System;

namespace NovoyaHope.Controllers
{
    [AllowAnonymous]
    public class PublicController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PublicController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET /public/viewsurvey/1
        public async Task<IActionResult> ViewSurvey(int id)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.AnswerOptions)
                .Include(s => s.Sections)
                .Include(s => s.Media)
                .FirstOrDefaultAsync(s => s.Id == id && s.IsPublished);

            if (survey == null)
            {
                return NotFound("Опрос не найден или не опубликован.");
            }

            // Map Survey -> PassSurveyViewModel, include questions and options
            var viewModel = new PassSurveyViewModel
            {
                Id = survey.Id,
                Title = survey.Title ?? "",
                Description = survey.Description ?? "",
                IsAnonymous = survey.IsAnonymous,
                Questions = (survey.Questions ?? new List<Question>())
                    .OrderBy(q => q.Order)
                    .Select(q => new PassQuestionViewModel
                    {
                        Id = q.Id,
                        Text = q.Text ?? "",
                        Type = q.Type,
                        IsRequired = q.IsRequired,
                        Order = q.Order,
                        Options = (q.AnswerOptions ?? new List<AnswerOption>())
                            .OrderBy(o => o.Order)
                            .Select(o => new PassAnswerOptionViewModel
                            {
                                Id = o.Id,
                                Text = o.Text ?? "",
                                Order = o.Order,
                                IsOther = o.IsOther
                            }).ToList()
                    }).ToList(),
                Sections = (survey.Sections ?? new List<Section>())
                    .OrderBy(s => s.Order)
                    .ToList(),
                Media = (survey.Media ?? new List<Media>())
                    .OrderBy(m => m.Order)
                    .ToList(),
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

            return View("Pass", viewModel);
        }

        // POST /public/submitresponse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitResponse(int SurveyId, IFormCollection form)
        {
            // Создаем модель из параметров формы
            var model = new SubmitResponseViewModel
            {
                SurveyId = SurveyId,
                Answers = new Dictionary<int, UserResponseData>()
            };

            // Обрабатываем данные формы вручную
            // Группируем по вопросам
            var questionGroups = form.Keys
                .Where(k => k.StartsWith("Answers[") && k.Contains("]"))
                .GroupBy(k =>
                {
                    var startIndex = k.IndexOf('[') + 1;
                    var endIndex = k.IndexOf(']');
                    if (startIndex > 0 && endIndex > startIndex)
                    {
                        if (int.TryParse(k.Substring(startIndex, endIndex - startIndex), out int qId))
                            return qId;
                    }
                    return -1;
                })
                .Where(g => g.Key > 0);

            foreach (var group in questionGroups)
            {
                var questionId = group.Key;
                var answerData = new UserResponseData
                {
                    SelectedOptionIds = new List<int>()
                };

                foreach (var key in group)
                {
                    var fieldName = key.Substring(key.IndexOf(']') + 2); // После "].TextAnswer" или "].SelectedOptionIds"
                    
                    if (fieldName == "TextAnswer")
                    {
                        answerData.TextAnswer = form[key].ToString();
                    }
                    else if (fieldName == "SelectedOptionIds")
                    {
                        // Для чекбоксов и радио - получаем все значения
                        var values = form[key];
                        foreach (var value in values)
                        {
                            if (int.TryParse(value, out int optionId))
                            {
                                answerData.SelectedOptionIds.Add(optionId);
                            }
                        }
                    }
                }

                model.Answers[questionId] = answerData;
            }

            // 1. Проверка существования и статуса опроса
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(s => s.Id == model.SurveyId);

            if (survey == null || !survey.IsPublished) 
            {
                return NotFound("Опрос не найден или не опубликован.");
            }

            // 2. Получение ID пользователя (если не анонимный)
            string? userId = null;
            if (!survey.IsAnonymous && User.Identity != null && User.Identity.IsAuthenticated)
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            // 3. Создание записи SurveyResponse
            var responseEntry = new SurveyResponse
            {
                SurveyId = model.SurveyId,
                SubmissionDate = DateTime.UtcNow,
                UserId = userId
            };

            // 4. Обработка всех ответов
            var userAnswers = new List<UserAnswer>();
            if (model.Answers != null)
            {
                foreach (var answerPair in model.Answers)
                {
                    var questionId = answerPair.Key;
                    var answerData = answerPair.Value;

                    var question = survey.Questions.FirstOrDefault(q => q.Id == questionId);
                    if (question == null) continue;

                    // Проверка обязательных вопросов
                    if (question.IsRequired)
                    {
                        bool hasAnswer = false;
                        if (question.Type == QuestionType.ShortText || question.Type == QuestionType.ParagraphText)
                        {
                            hasAnswer = !string.IsNullOrWhiteSpace(answerData.TextAnswer);
                        }
                        else
                        {
                            hasAnswer = answerData.SelectedOptionIds != null && answerData.SelectedOptionIds.Any();
                        }

                        if (!hasAnswer)
                        {
                            ModelState.AddModelError($"Answers[{questionId}]", $"Вопрос '{question.Text}' обязателен для заполнения.");
                            continue;
                        }
                    }

                    // Обработка текстовых ответов
                    if (question.Type == QuestionType.ShortText || question.Type == QuestionType.ParagraphText)
                    {
                        if (!string.IsNullOrWhiteSpace(answerData.TextAnswer))
                        {
                            var userAnswer = new UserAnswer
                            {
                                QuestionId = questionId,
                                TextAnswer = answerData.TextAnswer.Trim()
                            };
                            userAnswers.Add(userAnswer);
                        }
                    }
                    // Обработка одиночного выбора (Radio) или шкалы
                    else if (question.Type == QuestionType.SingleChoice || question.Type == QuestionType.Scale)
                    {
                        if (answerData.SelectedOptionIds != null && answerData.SelectedOptionIds.Any())
                        {
                            var selectedOptionId = answerData.SelectedOptionIds.First();
                            // Проверка, что опция принадлежит этому вопросу
                            if (question.AnswerOptions.Any(o => o.Id == selectedOptionId))
                            {
                                var userAnswer = new UserAnswer
                                {
                                    QuestionId = questionId,
                                    SelectedOptionId = selectedOptionId,
                                    TextAnswer = null // Явно устанавливаем null для вопросов с вариантами ответов
                                };
                                userAnswers.Add(userAnswer);
                            }
                        }
                    }
                    // Обработка множественного выбора (Checkbox)
                    else if (question.Type == QuestionType.MultipleChoice)
                    {
                        if (answerData.SelectedOptionIds != null && answerData.SelectedOptionIds.Any())
                        {
                            foreach (var optionId in answerData.SelectedOptionIds)
                            {
                                // Проверка, что опция принадлежит этому вопросу
                                if (question.AnswerOptions.Any(o => o.Id == optionId))
                                {
                                    var userAnswer = new UserAnswer
                                    {
                                        QuestionId = questionId,
                                        SelectedOptionId = optionId,
                                        TextAnswer = null // Явно устанавливаем null для вопросов с вариантами ответов
                                    };
                                    userAnswers.Add(userAnswer);
                                }
                            }
                        }
                    }
                }
            }

            // 5. Сохранение ответа
            if (ModelState.IsValid)
            {
                _context.SurveyResponses.Add(responseEntry);
                await _context.SaveChangesAsync(); // Сохраняем SurveyResponse, чтобы получить Id

                // Устанавливаем ResponseId для всех UserAnswer
                foreach (var userAnswer in userAnswers)
                {
                    userAnswer.ResponseId = responseEntry.Id;
                    _context.UserAnswers.Add(userAnswer);
                }
                
                await _context.SaveChangesAsync(); // Сохраняем UserAnswer
                return RedirectToAction(nameof(ThankYou));
            }
            else
            {
                // Если есть ошибки валидации, возвращаем к форме
                return RedirectToAction(nameof(ViewSurvey), new { id = model.SurveyId });
            }
        }

        public IActionResult ThankYou()
        {
            return View();
        }
    }
}