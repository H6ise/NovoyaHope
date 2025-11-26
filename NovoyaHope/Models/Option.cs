using System;

namespace NovoyaHope.Models
{
    public class Option
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string Text { get; set; }
        public int Order { get; set; }
        public bool IsOther { get; set; }

        // Навигационные свойства
        public Question? Question { get; set; }
    }
}
