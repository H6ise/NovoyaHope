using System.ComponentModel.DataAnnotations;

namespace NovoyaHope.Models
{
    /// <summary>
    /// Тип медиа-контента
    /// </summary>
    public enum MediaType
    {
        Image,  // Изображение
        Video   // Видео
    }
    
    /// <summary>
    /// Медиа-элемент (изображение или видео) в опросе
    /// </summary>
    public class Media
    {
        public int Id { get; set; }
        
        public int SurveyId { get; set; }
        
        public MediaType Type { get; set; }
        
        /// <summary>
        /// URL или путь к файлу
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Url { get; set; } = string.Empty;
        
        /// <summary>
        /// Альтернативный текст для изображения / заголовок для видео
        /// </summary>
        [MaxLength(200)]
        public string? Title { get; set; }
        
        /// <summary>
        /// Описание медиа
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }
        
        /// <summary>
        /// Порядок отображения в опросе
        /// </summary>
        public int Order { get; set; }
        
        /// <summary>
        /// ID вопроса, к которому привязан медиа-элемент (если применимо)
        /// </summary>
        public int? QuestionId { get; set; }
        
        // Навигационные свойства
        public Survey? Survey { get; set; }
        public Question? Question { get; set; }
    }
}

