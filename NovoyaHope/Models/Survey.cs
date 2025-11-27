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

        // Связь с создателем
        public required string CreatorId { get; set; }
        public ApplicationUser? Creator { get; set; }

        // Навигационные свойства
        public ICollection<Question>? Questions { get; set; }
        public ICollection<SurveyResponse>? Responses { get; set; }
    }
}