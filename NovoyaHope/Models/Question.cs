using System.Collections.Generic;

namespace NovoyaHope.Models
{
    public enum QuestionType
    {
        SingleChoice = 1,    // Один вариант (Radio button)
        MultipleChoice = 2,  // Несколько вариантов (Checkbox)
        ShortText = 3,       // Короткий текст
        ParagraphText = 4,   // Длинный текст
        Scale = 5            // Шкала (1-5, 1-10 и т.д.)
    }

    public class Question
    {
        public int Id { get; set; }
        public int SurveyId { get; set; }
        public Survey Survey { get; set; }

        public int Order { get; set; } // Порядок отображения
        public string Text { get; set; }
        public QuestionType Type { get; set; }
        public bool IsRequired { get; set; } = true;

        // Варианты ответов (для Choice и Scale)
        public ICollection<AnswerOption> AnswerOptions { get; set; }
    }
}