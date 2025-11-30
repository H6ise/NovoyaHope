using NovoyaHope.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace NovoyaHope.Models.ViewModels
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

        // Настройки теста
        public bool IsTestMode { get; set; } = false;
        public GradePublicationType GradePublication { get; set; } = GradePublicationType.AfterManualReview;
        public bool ShowIncorrectAnswers { get; set; } = true;
        public bool ShowCorrectAnswers { get; set; } = true;
        public bool ShowPoints { get; set; } = true;
        public int DefaultMaxPoints { get; set; } = 0;

        // Настройки темы
        public string? ThemeColor { get; set; } = "#673AB7";
        public string? BackgroundColor { get; set; } = "#F3E5F5";
        public string? HeaderImagePath { get; set; }
        public string? HeaderFontFamily { get; set; } = "Courier New";
        public int? HeaderFontSize { get; set; } = 24;
        public string? QuestionFontFamily { get; set; } = "Roboto";
        public int? QuestionFontSize { get; set; } = 12;
        public string? TextFontFamily { get; set; } = "Roboto";
        public int? TextFontSize { get; set; } = 11;

        // Список вопросов в текущем опросе
        public List<SaveQuestionViewModel> Questions { get; set; }
        
        // Список разделов в опросе
        public List<SaveSectionViewModel> Sections { get; set; }
        
        // Список медиа-элементов (изображения и видео)
        public List<SaveMediaViewModel> Media { get; set; }
    }
    
    // ViewModel для раздела
    public class SaveSectionViewModel
    {
        public int? Id { get; set; }
        [Required]
        public int Order { get; set; }
        [Required(ErrorMessage = "Укажите заголовок раздела.")]
        public string Title { get; set; }
        public string? Description { get; set; }
    }
    
    // ViewModel для медиа-элемента
    public class SaveMediaViewModel
    {
        public int? Id { get; set; }
        [Required]
        public int Order { get; set; }
        public MediaType Type { get; set; }
        [Required(ErrorMessage = "Укажите URL или загрузите файл.")]
        public string Url { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? QuestionId { get; set; }
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