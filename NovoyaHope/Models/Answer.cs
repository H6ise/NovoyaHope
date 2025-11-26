using System;

namespace NovoyaHope.Models
{
    public class Answer
    {
        public int Id { get; set; }
        public int ResponseId { get; set; }
        public int QuestionId { get; set; }

        // Хранение фактического ответа: 
        // - Текст для Short/ParagraphText
        // - ID выбранного Option (или JSON/строка через запятую для MultipleChoice)
        public string Value { get; set; }

        // Навигационные свойства
        public SurveyResponse? Response { get; set; }
        public Question? Question { get; set; }
    }
}
