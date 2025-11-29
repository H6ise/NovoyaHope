using NovoyaHope.Models;
using System.Collections.Generic;

namespace NovoyaHope.Models.ViewModels
{
    // Модель представления, используемая для передачи всех данных 
    // конструктора (Survey и его коллекция Questions) на страницу.
    public class SurveyConstructorViewModel
    {
        // Основной объект опроса (содержит Title, Description и Id).
        public Survey Survey { get; set; }

        // Коллекция вопросов, связанных с этим опросом. 
        // Важно: в ASP.NET Core MVC при Model Binding это будет массив 
        // или список, где каждое Question будет иметь свои Options.
        public List<Question> Questions { get; set; }

        // Данные ответов для вкладки "Ответы"
        public SurveyResultsViewModel? ResultsData { get; set; }

        // Дополнительные поля для обработки удалений на стороне клиента (опционально)
        // public string DeletedElements { get; set; }
    }
}