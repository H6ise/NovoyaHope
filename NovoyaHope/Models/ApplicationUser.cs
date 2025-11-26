using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace NovoyaHope.Models
{
    // Расширяем стандартный IdentityUser, чтобы добавить связи с опросами
    public class ApplicationUser : IdentityUser
    {
        // Опросы, созданные этим пользователем
        public ICollection<Survey> CreatedSurveys { get; set; }

        // Ответы, которые этот пользователь оставил в других опросах
        // (Используется, если опрос не анонимный)
        public ICollection<SurveyResponse> Responses { get; set; }
    }
}