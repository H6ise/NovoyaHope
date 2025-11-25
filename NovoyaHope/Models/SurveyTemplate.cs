using System.Collections.Generic;

namespace NovoyaHope.Models
{
    public class SurveyTemplate
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public SurveyType Type { get; set; }

        public string CreatorId { get; set; }
        // Примечание: Можно добавить связь с ApplicationUser, но здесь опущено для простоты

        // Шаблонные вопросы
        public ICollection<TemplateQuestion> Questions { get; set; }
    }

    // Вспомогательные модели для вопросов и опций шаблона
    public class TemplateQuestion
    {
        public int Id { get; set; }
        public int SurveyTemplateId { get; set; }
        public SurveyTemplate SurveyTemplate { get; set; }

        public int Order { get; set; }
        public string Text { get; set; }
        public QuestionType Type { get; set; }

        public ICollection<TemplateAnswerOption> AnswerOptions { get; set; }
    }

    public class TemplateAnswerOption
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public TemplateQuestion Question { get; set; }

        public string Text { get; set; }
        public int Order { get; set; }
    }
}