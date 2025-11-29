using System;
using System.Collections.Generic;

namespace NovoyaHope.Models
{
    public enum SurveyType
    {
        Poll = 1,       // Голосование (обычно один вопрос)
        Questionnaire = 2 // Много вопросов
    }

    public class Survey
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }

        // Настройки
        public SurveyType Type { get; set; }
        public bool IsPublished { get; set; } = false;
        public bool IsAnonymous { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public DateTime? EndDate { get; set; } // Опциональная дата окончания

        // Настройки теста
        public bool IsTestMode { get; set; } = false;
        public GradePublicationType GradePublication { get; set; } = GradePublicationType.AfterManualReview;
        public bool ShowIncorrectAnswers { get; set; } = true;
        public bool ShowCorrectAnswers { get; set; } = true;
        public bool ShowPoints { get; set; } = true;
        public int DefaultMaxPoints { get; set; } = 0;

        // Настройки темы
        public string? ThemeColor { get; set; } = "#673AB7"; // Фиолетовый по умолчанию
        public string? BackgroundColor { get; set; } = "#F3E5F5"; // Светло-фиолетовый по умолчанию
        public string? HeaderImagePath { get; set; }
        public string? HeaderFontFamily { get; set; } = "Courier New";
        public int? HeaderFontSize { get; set; } = 24;
        public string? QuestionFontFamily { get; set; } = "Roboto";
        public int? QuestionFontSize { get; set; } = 12;
        public string? TextFontFamily { get; set; } = "Roboto";
        public int? TextFontSize { get; set; } = 11;

        // Связь с создателем
        public required string CreatorId { get; set; }
        public ApplicationUser? Creator { get; set; }

        // Навигационные свойства
        public ICollection<Question>? Questions { get; set; }
        public ICollection<SurveyResponse>? Responses { get; set; }
    }

    public enum GradePublicationType
    {
        Immediately = 1,      // Сразу после отправки формы
        AfterManualReview = 2 // После ручной проверки
    }
}