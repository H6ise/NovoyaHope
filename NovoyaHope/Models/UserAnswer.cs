namespace NovoyaHope.Models
{
    public class UserAnswer
    {
        public int Id { get; set; }
        public int ResponseId { get; set; }
        public SurveyResponse Response { get; set; }

        public int QuestionId { get; set; }
        public Question Question { get; set; }

        // Для текстовых ответов (ShortText, ParagraphText)
        public string TextAnswer { get; set; }

        // Для SingleChoice/Scale (ID выбранного варианта)
        public int? SelectedOptionId { get; set; }
        public AnswerOption SelectedOption { get; set; }
    }
}