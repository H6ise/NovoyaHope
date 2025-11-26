using NovoyaHope.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace NovoyaHope.Models
{
    public enum QuestionType
    {
        ShortText,        // Текст (строка)
        ParagraphText,    // Текст (абзац)
        SingleChoice,     // Один из списка (радио-кнопки)
        MultipleChoice,   // Несколько из списка (чек-боксы)
        Dropdown,         // Раскрывающийся список
        FileUpload,       // Загрузка файлов
        Scale,            // Шкала (линейная)
        Rating,           // Оценка (звезды)
        CheckboxGrid,     // Сетка флажков
        RadioGrid,        // Сетка (множественный выбор)
        Date,             // Дата
        Time              // Время
    }

    public class Question
    {
        public int Id { get; set; }
        public int SurveyId { get; set; }
        public string Text { get; set; }
        public QuestionType Type { get; set; }
        public bool IsRequired { get; set; }
        public int Order { get; set; }

        // Навигационные свойства
        public Survey? Survey { get; set; }
        public ICollection<AnswerOption>? AnswerOptions { get; set; } // Варианты для выбора

        // Compatibility alias for views that reference "Options"
        [NotMapped]
        public ICollection<AnswerOption>? Options
        {
            get => AnswerOptions;
            set => AnswerOptions = value;
        }
        public ICollection<Answer>? Answers { get; set; } // Ответы на этот вопрос
    }
}