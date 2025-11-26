using Microsoft.EntityFrameworkCore;
using NovoyaHope.Data;
using NovoyaHope.Models;
using NovoyaHope.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NovoyaHope.Services
{
    public class SurveyService : ISurveyService
    {
        private readonly ApplicationDbContext _context;

        public SurveyService(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- CRUD: СОХРАНЕНИЕ / ОБНОВЛЕНИЕ ---

        public async Task<int> SaveSurveyAsync(SaveSurveyViewModel model, string userId)
        {
            Survey survey;

            // 1. Определение: Создание или Обновление?
            if (model.Id.HasValue && model.Id.Value > 0)
            {
                // ОБНОВЛЕНИЕ
                survey = await _context.Surveys
                    .Include(s => s.Questions)
                        .ThenInclude(q => q.AnswerOptions)
                    .FirstOrDefaultAsync(s => s.Id == model.Id.Value && s.CreatorId == userId);

                if (survey == null)
                    throw new UnauthorizedAccessException("Опрос не найден или у вас нет прав на его редактирование.");
            }
            else
            {
                // СОЗДАНИЕ
                survey = new Survey
                {
                    CreatorId = userId,
                    CreatedDate = DateTime.UtcNow,
                    Questions = new List<Question>()
                };
                _context.Surveys.Add(survey);
            }

            // 2. Обновление метаданных
            survey.Title = model.Title;
            survey.Description = model.Description;
            survey.Type = model.Type;
            survey.IsAnonymous = model.IsAnonymous;
            survey.EndDate = model.EndDate;
            survey.IsPublished = model.IsPublished;

            // 3. Обновление Вопросов (Сложная логика синхронизации)

            // Удаление вопросов, которые были удалены в конструкторе
            var modelQuestionIds = model.Questions.Where(q => q.Id.HasValue).Select(q => q.Id.Value).ToList();
            var questionsToRemove = survey.Questions.Where(q => !modelQuestionIds.Contains(q.Id)).ToList();
            _context.Questions.RemoveRange(questionsToRemove);

            foreach (var qModel in model.Questions)
            {
                Question question;

                if (qModel.Id.HasValue && qModel.Id.Value > 0)
                {
                    // Обновление существующего вопроса
                    question = survey.Questions.FirstOrDefault(q => q.Id == qModel.Id.Value);
                    if (question == null) continue;
                }
                else
                {
                    // Добавление нового вопроса
                    question = new Question { SurveyId = survey.Id };
                    survey.Questions.Add(question);
                }

                // Обновление полей вопроса
                question.Text = qModel.Text;
                question.Type = qModel.Type;
                question.Order = qModel.Order;
                question.IsRequired = qModel.IsRequired;

                // Обновление Вариантов Ответа (Сложная логика синхронизации)
                // Удаление опций
                var modelOptionIds = qModel.Options?.Where(o => o.Id.HasValue).Select(o => o.Id.Value).ToList() ?? new List<int>();
                var optionsToRemove = question.AnswerOptions.Where(o => !modelOptionIds.Contains(o.Id)).ToList();
                _context.AnswerOptions.RemoveRange(optionsToRemove);

                if (qModel.Options != null)
                {
                    foreach (var oModel in qModel.Options)
                    {
                        AnswerOption option;
                        if (oModel.Id.HasValue && oModel.Id.Value > 0)
                        {
                            // Обновление существующей опции
                            option = question.AnswerOptions.FirstOrDefault(o => o.Id == oModel.Id.Value);
                            if (option == null) continue;
                        }
                        else
                        {
                            // Добавление новой опции
                            option = new AnswerOption { QuestionId = question.Id };
                            question.AnswerOptions.Add(option);
                        }

                        option.Text = oModel.Text;
                        option.Order = oModel.Order;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return survey.Id;
        }

        // --- CRUD: ПОЛУЧЕНИЕ ДЛЯ РЕДАКТИРОВАНИЯ ---

        public async Task<SaveSurveyViewModel> GetSurveyForEditAsync(int surveyId, string userId)
        {
            var survey = await _context.Surveys
                .AsNoTracking() // Только для чтения
                .Include(s => s.Questions.OrderBy(q => q.Order))
                    .ThenInclude(q => q.AnswerOptions.OrderBy(o => o.Order))
                .FirstOrDefaultAsync(s => s.Id == surveyId && s.CreatorId == userId);

            if (survey == null) return null;

            // Здесь должна быть логика маппинга Survey -> SaveSurveyViewModel
            // (Для краткости опускаем, используя AutoMapper или ручной маппинг)
            var viewModel = new SaveSurveyViewModel
            {
                Id = survey.Id,
                Title = survey.Title,
                // ... и т.д.
                Questions = survey.Questions.Select(q => new SaveQuestionViewModel
                {
                    Id = q.Id,
                    Text = q.Text,
                    // ...
                    Options = q.AnswerOptions.Select(o => new SaveAnswerOptionViewModel
                    {
                        Id = o.Id,
                        Text = o.Text,
                        // ...
                    }).ToList()
                }).ToList()
            };

            return viewModel;
        }

        // --- УПРАВЛЕНИЕ СТАТУСОМ ---

        public async Task<bool> ChangePublicationStatusAsync(int surveyId, string userId, bool isPublished)
        {
            var survey = await _context.Surveys.FirstOrDefaultAsync(s => s.Id == surveyId && s.CreatorId == userId);

            if (survey == null) return false;

            if (isPublished && !await IsSurveyValidForPublishingAsync(surveyId, userId))
            {
                // Дополнительная проверка, прежде чем публиковать
                return false;
            }

            survey.IsPublished = isPublished;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsSurveyValidForPublishingAsync(int surveyId, string userId)
        {
            // Проверка наличия хотя бы одного вопроса
            return await _context.Questions.AnyAsync(q => q.SurveyId == surveyId && q.Survey.CreatorId == userId);
        }

        // --- CRUD: УДАЛЕНИЕ ---

        public async Task<bool> DeleteSurveyAsync(int surveyId, string userId)
        {
            var survey = await _context.Surveys.FirstOrDefaultAsync(s => s.Id == surveyId && s.CreatorId == userId);

            if (survey == null) return false;

            _context.Surveys.Remove(survey);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}