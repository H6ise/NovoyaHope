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
                    .Include(s => s.Sections)
                    .Include(s => s.Media)
                    .FirstOrDefaultAsync(s => s.Id == model.Id.Value && s.CreatorId == userId);

                if (survey == null)
                    throw new UnauthorizedAccessException("Опрос не найден или у вас нет прав на его редактирование.");
            }
            else
            {
                // СОЗДАНИЕ
                survey = new Survey
                {
                    Title = string.Empty,
                    Description = string.Empty,
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

            // 2.1. Обновление настроек теста
            survey.IsTestMode = model.IsTestMode;
            survey.GradePublication = model.GradePublication;
            survey.ShowIncorrectAnswers = model.ShowIncorrectAnswers;
            survey.ShowCorrectAnswers = model.ShowCorrectAnswers;
            survey.ShowPoints = model.ShowPoints;
            survey.DefaultMaxPoints = model.DefaultMaxPoints;

            // 2.2. Обновление настроек темы
            survey.ThemeColor = model.ThemeColor;
            survey.BackgroundColor = model.BackgroundColor;
            survey.HeaderImagePath = model.HeaderImagePath;
            survey.HeaderFontFamily = model.HeaderFontFamily;
            survey.HeaderFontSize = model.HeaderFontSize;
            survey.QuestionFontFamily = model.QuestionFontFamily;
            survey.QuestionFontSize = model.QuestionFontSize;
            survey.TextFontFamily = model.TextFontFamily;
            survey.TextFontSize = model.TextFontSize;

            // Если это новый опрос, сначала сохраняем его, чтобы получить Id
            if (survey.Id == 0)
            {
                await _context.SaveChangesAsync();
            }

            // 3. Обновление Вопросов (Сложная логика синхронизации)
            // Инициализируем коллекцию вопросов, если она null
            if (survey.Questions == null)
            {
                survey.Questions = new List<Question>();
            }

            // Инициализируем список вопросов из модели, если он null
            var questionsList = model.Questions ?? new List<SaveQuestionViewModel>();

            // Удаление вопросов, которые были удалены в конструкторе
            var modelQuestionIds = questionsList.Where(q => q.Id.HasValue).Select(q => q.Id.Value).ToList();
            var questionsToRemove = survey.Questions.Where(q => !modelQuestionIds.Contains(q.Id)).ToList();
            _context.Questions.RemoveRange(questionsToRemove);

            foreach (var qModel in questionsList)
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
                // Инициализируем коллекцию опций, если она null
                if (question.AnswerOptions == null)
                {
                    question.AnswerOptions = new List<AnswerOption>();
                }
                
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
                    // Инициализируем коллекцию опций, если она null
                    if (question.AnswerOptions == null)
                    {
                        question.AnswerOptions = new List<AnswerOption>();
                    }
                    option = new AnswerOption { QuestionId = question.Id };
                    question.AnswerOptions.Add(option);
                }

                        option.Text = oModel.Text;
                        option.Order = oModel.Order;
                    }
                }
            }

            // 4. Обновление Разделов (Sections)
            if (survey.Sections == null)
            {
                survey.Sections = new List<Section>();
            }

            var sectionsList = model.Sections ?? new List<SaveSectionViewModel>();
            
            // Удаление разделов, которые были удалены в конструкторе
            var modelSectionIds = sectionsList.Where(s => s.Id.HasValue).Select(s => s.Id.Value).ToList();
            var sectionsToRemove = survey.Sections.Where(s => !modelSectionIds.Contains(s.Id)).ToList();
            _context.Sections.RemoveRange(sectionsToRemove);

            foreach (var sModel in sectionsList)
            {
                Section section;
                if (sModel.Id.HasValue && sModel.Id.Value > 0)
                {
                    // Обновление существующего раздела
                    section = survey.Sections.FirstOrDefault(s => s.Id == sModel.Id.Value);
                    if (section == null) continue;
                }
                else
                {
                    // Добавление нового раздела
                    section = new Section { SurveyId = survey.Id };
                    survey.Sections.Add(section);
                }

                section.Title = sModel.Title;
                section.Description = sModel.Description;
                section.Order = sModel.Order;
            }

            // 5. Обновление Медиа-элементов (Images и Videos)
            if (survey.Media == null)
            {
                survey.Media = new List<Media>();
            }

            var mediaList = model.Media ?? new List<SaveMediaViewModel>();
            
            // Удаление медиа, которые были удалены в конструкторе
            var modelMediaIds = mediaList.Where(m => m.Id.HasValue).Select(m => m.Id.Value).ToList();
            var mediaToRemove = survey.Media.Where(m => !modelMediaIds.Contains(m.Id)).ToList();
            _context.Media.RemoveRange(mediaToRemove);

            foreach (var mModel in mediaList)
            {
                Media media;
                if (mModel.Id.HasValue && mModel.Id.Value > 0)
                {
                    // Обновление существующего медиа
                    media = survey.Media.FirstOrDefault(m => m.Id == mModel.Id.Value);
                    if (media == null) continue;
                }
                else
                {
                    // Добавление нового медиа
                    media = new Media { SurveyId = survey.Id };
                    survey.Media.Add(media);
                }

                media.Type = mModel.Type;
                media.Url = mModel.Url;
                media.Title = mModel.Title;
                media.Description = mModel.Description;
                media.Order = mModel.Order;
                media.QuestionId = mModel.QuestionId;
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
                .Include(s => s.Sections.OrderBy(s => s.Order))
                .Include(s => s.Media.OrderBy(m => m.Order))
                .FirstOrDefaultAsync(s => s.Id == surveyId && s.CreatorId == userId);

            if (survey == null) return null;

            // Здесь должна быть логика маппинга Survey -> SaveSurveyViewModel
            // (Для краткости опускаем, используя AutoMapper или ручной маппинг)
            var viewModel = new SaveSurveyViewModel
            {
                Id = survey.Id,
                Title = survey.Title,
                Description = survey.Description,
                Type = survey.Type,
                IsPublished = survey.IsPublished,
                EndDate = survey.EndDate,
                IsAnonymous = survey.IsAnonymous,
                IsTestMode = survey.IsTestMode,
                GradePublication = survey.GradePublication,
                ShowIncorrectAnswers = survey.ShowIncorrectAnswers,
                ShowCorrectAnswers = survey.ShowCorrectAnswers,
                ShowPoints = survey.ShowPoints,
                DefaultMaxPoints = survey.DefaultMaxPoints,
                ThemeColor = survey.ThemeColor,
                BackgroundColor = survey.BackgroundColor,
                HeaderImagePath = survey.HeaderImagePath,
                HeaderFontFamily = survey.HeaderFontFamily,
                HeaderFontSize = survey.HeaderFontSize,
                QuestionFontFamily = survey.QuestionFontFamily,
                QuestionFontSize = survey.QuestionFontSize,
                TextFontFamily = survey.TextFontFamily,
                TextFontSize = survey.TextFontSize,
                Questions = survey.Questions.Select(q => new SaveQuestionViewModel
                {
                    Id = q.Id,
                    Text = q.Text,
                    Type = q.Type,
                    Order = q.Order,
                    IsRequired = q.IsRequired,
                    Options = q.AnswerOptions.Select(o => new SaveAnswerOptionViewModel
                    {
                        Id = o.Id,
                        Text = o.Text,
                        Order = o.Order
                    }).ToList()
                }).ToList(),
                Sections = survey.Sections.Select(s => new SaveSectionViewModel
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    Order = s.Order
                }).ToList(),
                Media = survey.Media.Select(m => new SaveMediaViewModel
                {
                    Id = m.Id,
                    Type = m.Type,
                    Url = m.Url,
                    Title = m.Title,
                    Description = m.Description,
                    Order = m.Order,
                    QuestionId = m.QuestionId
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