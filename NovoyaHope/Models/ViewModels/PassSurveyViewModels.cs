using NovoyaHope.Models;
using System.Collections.Generic;

namespace NovoyaHope.Models.ViewModels.PassSurveyViewModels
{
    public class PassSurveyViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsAnonymous { get; set; }
        public List<PassQuestionViewModel> Questions { get; set; }
    }

    public class PassQuestionViewModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public QuestionType Type { get; set; }
        public bool IsRequired { get; set; }
        public List<PassAnswerOptionViewModel> Options { get; set; }
    }

    public class PassAnswerOptionViewModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int Order { get; set; }
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