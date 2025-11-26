using System.ComponentModel.DataAnnotations;

namespace NovoyaHope.Models
{
    /// <summary>
    /// Раздел опроса - используется для группировки вопросов
    /// </summary>
    public class Section
    {
        public int Id { get; set; }
        
        public int SurveyId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string? Description { get; set; }
        
        /// <summary>
        /// Порядок отображения в опросе
        /// </summary>
        public int Order { get; set; }
        
        // Навигационное свойство
        public Survey? Survey { get; set; }
    }
}

