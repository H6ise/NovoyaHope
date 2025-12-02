using NovoyaHope.Models;
using System.Collections.Generic;

namespace NovoyaHope.Models.ViewModels
{
    public class PassSurveyViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsAnonymous { get; set; }
        public List<PassQuestionViewModel> Questions { get; set; }
        public List<Section> Sections { get; set; } = new List<Section>();
        public List<Media> Media { get; set; } = new List<Media>();

        // Настройки темы
        public string? ThemeColor { get; set; }
        public string? BackgroundColor { get; set; }
        public string? HeaderImagePath { get; set; }
        public string? HeaderFontFamily { get; set; }
        public int? HeaderFontSize { get; set; }
        public string? QuestionFontFamily { get; set; }
        public int? QuestionFontSize { get; set; }
        public string? TextFontFamily { get; set; }
        public int? TextFontSize { get; set; }
    }

    public class PassQuestionViewModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public QuestionType Type { get; set; }
        public bool IsRequired { get; set; }
        public int Order { get; set; }
        public List<PassAnswerOptionViewModel> Options { get; set; }
    }

    public class PassAnswerOptionViewModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int Order { get; set; }
        public bool IsOther { get; set; }
    }

    // --- Модель для приема ответов ---

    // Используется в PublicController.SubmitResponse
    public class SubmitResponseViewModel
    {
        public int SurveyId { get; set; }
        // Словарь для привязки данных формы: Answers[ID_вопроса].Поле
        public Dictionary<int, UserResponseData> Answers { get; set; }
    }

    // Данные ответа на один вопрос
    public class UserResponseData
    {
        // Для текстовых ответов
        public string TextAnswer { get; set; }

        // Для выбора (Single/Multiple/Scale). List нужен для чекбоксов (MultipleChoice).
        public List<int> SelectedOptionIds { get; set; }
    }
}