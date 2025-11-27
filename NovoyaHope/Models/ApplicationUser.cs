using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace NovoyaHope.Models
{
    // Расширяем стандартный IdentityUser, чтобы добавить связи с опросами
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            CreatedSurveys = new List<Survey>();
            Responses = new List<SurveyResponse>();
        }

        // Опросы, созданные этим пользователем
        public ICollection<Survey> CreatedSurveys { get; set; }

        // Ответы, которые этот пользователь оставил в других опросах
        // (Используется, если опрос не анонимный)
        public ICollection<SurveyResponse> Responses { get; set; }

        // Дополнительные поля профиля
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfileImagePath { get; set; }
        public bool ShowPhoneToPublic { get; set; } = false;
    }
}