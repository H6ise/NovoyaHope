using NovoyaHope.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace NovoyaHope.Models.ViewModels.SurveyViewModels
{
    public class SaveSurveyViewModel
    {
        public int? Id { get; set; } // Null для нового опроса

        [Required(ErrorMessage = "Укажите заголовок.")]
        public string Title { get; set; }

        public string Description { get; set; }

        public SurveyType Type { get; set; }

        public bool IsPublished { get; set; } = false;
        public DateTime? EndDate { get; set; }
        public bool IsAnonymous { get; set; }

        // Список вопросов в текущем опросе
        public List<SaveQuestionViewModel> Questions { get; set; }
    }

    // Часть SaveSurveyViewModel: Модель для одного вопроса
    public class SaveQuestionViewModel
    {
        public int? Id { get; set; }
        [Required]
        public int Order { get; set; }
        [Required(ErrorMessage = "Текст вопроса не может быть пустым.")]
        public string Text { get; set; }
        public QuestionType Type { get; set; }
        public bool IsRequired { get; set; }
        public List<SaveAnswerOptionViewModel> Options { get; set; }
    }

    // Часть SaveQuestionViewModel: Модель для одного варианта ответа
    public class SaveAnswerOptionViewModel
    {
        public int? Id { get; set; }
        [Required(ErrorMessage = "Текст варианта не может быть пустым.")]
        public string Text { get; set; }
        [Required]
        public int Order { get; set; }
    }

    // Модель для отображения карточек на главной странице/списке
    public class TemplateDisplayViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string TypeDescription { get; set; }
        public string PreviewBackgroundColor { get; set; }
    }

    public class HomeIndexViewModel
    {
        public IEnumerable<TemplateDisplayViewModel> TemplatePreviews { get; set; }
    }
}