using NovoyaHope.Models;
using System;
using System.Collections.Generic;

namespace NovoyaHope.Models
{
    public class SurveyResponse
    {
        public int Id { get; set; }
        public int SurveyId { get; set; }
        public Survey? Survey { get; set; }

        public DateTime SubmissionDate { get; set; }

        // ID пользователя (NULL, если опрос анонимный)
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        // Конкретные ответы на вопросы
        public ICollection<UserAnswer>? UserAnswers { get; set; }
    }
}
