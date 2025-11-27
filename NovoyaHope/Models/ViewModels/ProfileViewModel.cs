using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace NovoyaHope.Models.ViewModels
{
    /// <summary>
    /// ViewModel для отображения и редактирования профиля пользователя
    /// </summary>
    public class ProfileViewModel
    {
        [Display(Name = "Имя")]
        [StringLength(50, ErrorMessage = "Имя не должно превышать 50 символов")]
        [RegularExpression(@"^[а-яА-ЯёЁa-zA-Z\s-]+$", ErrorMessage = "Имя может содержать только буквы, пробелы и дефисы")]
        public string? FirstName { get; set; }

        [Display(Name = "Фамилия")]
        [StringLength(50, ErrorMessage = "Фамилия не должна превышать 50 символов")]
        [RegularExpression(@"^[а-яА-ЯёЁa-zA-Z\s-]+$", ErrorMessage = "Фамилия может содержать только буквы, пробелы и дефисы")]
        public string? LastName { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        [StringLength(256, ErrorMessage = "Email не должен превышать 256 символов")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Телефон")]
        [Phone(ErrorMessage = "Некорректный формат телефона")]
        [RegularExpression(@"^\+?[0-9\s\-\(\)]{10,20}$", ErrorMessage = "Телефон должен содержать от 10 до 20 цифр")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Показывать телефон всем")]
        public bool ShowPhoneToPublic { get; set; }

        [Display(Name = "Фото профиля")]
        public IFormFile? ProfileImage { get; set; }

        /// <summary>
        /// Текущий путь к фото профиля
        /// </summary>
        public string? ProfileImagePath { get; set; }

        /// <summary>
        /// Получить полное имя пользователя
        /// </summary>
        public string GetFullName()
        {
            if (!string.IsNullOrWhiteSpace(FirstName) || !string.IsNullOrWhiteSpace(LastName))
            {
                return $"{FirstName} {LastName}".Trim();
            }
            return Email;
        }

        /// <summary>
        /// Получить инициалы пользователя для аватара
        /// </summary>
        public string GetInitials()
        {
            var initials = "";
            
            if (!string.IsNullOrWhiteSpace(FirstName))
                initials += FirstName[0];
            
            if (!string.IsNullOrWhiteSpace(LastName))
                initials += LastName[0];
            
            if (string.IsNullOrWhiteSpace(initials) && !string.IsNullOrWhiteSpace(Email))
                initials = Email[0].ToString().ToUpper();
            
            return initials.ToUpper();
        }

        /// <summary>
        /// Проверка, заполнен ли профиль полностью
        /// </summary>
        public bool IsProfileComplete()
        {
            return !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(PhoneNumber) &&
                   !string.IsNullOrWhiteSpace(ProfileImagePath);
        }

        /// <summary>
        /// Процент заполненности профиля
        /// </summary>
        public int GetProfileCompleteness()
        {
            int completedFields = 0;
            int totalFields = 5;

            if (!string.IsNullOrWhiteSpace(FirstName)) completedFields++;
            if (!string.IsNullOrWhiteSpace(LastName)) completedFields++;
            if (!string.IsNullOrWhiteSpace(Email)) completedFields++;
            if (!string.IsNullOrWhiteSpace(PhoneNumber)) completedFields++;
            if (!string.IsNullOrWhiteSpace(ProfileImagePath)) completedFields++;

            return (int)((double)completedFields / totalFields * 100);
        }
    }

    /// <summary>
    /// ViewModel для изменения пароля пользователя
    /// </summary>
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Введите текущий пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Текущий пароль")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите новый пароль")]
        [StringLength(100, ErrorMessage = "Пароль должен быть не менее {2} символов", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$", 
            ErrorMessage = "Пароль должен содержать минимум одну заглавную букву, одну строчную букву и одну цифру")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Подтвердите новый пароль")]
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// Проверка надежности пароля (0-100)
        /// </summary>
        public static int GetPasswordStrength(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return 0;

            int strength = 0;

            // Длина
            if (password.Length >= 8) strength += 25;
            else strength += password.Length * 3;

            // Содержит строчные буквы
            if (System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-z]")) strength += 15;

            // Содержит заглавные буквы
            if (System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]")) strength += 15;

            // Содержит цифры
            if (System.Text.RegularExpressions.Regex.IsMatch(password, @"\d")) strength += 15;

            // Содержит спецсимволы
            if (System.Text.RegularExpressions.Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]")) strength += 20;

            // Разнообразие символов
            var uniqueChars = password.Distinct().Count();
            if (uniqueChars > password.Length * 0.7) strength += 10;

            return Math.Min(strength, 100);
        }

        /// <summary>
        /// Получить текстовое описание надежности пароля
        /// </summary>
        public static string GetPasswordStrengthText(int strength)
        {
            return strength switch
            {
                < 30 => "Слабый",
                < 60 => "Средний",
                < 80 => "Хороший",
                _ => "Отличный"
            };
        }
    }

    /// <summary>
    /// ViewModel для отображения публичного профиля пользователя
    /// </summary>
    public class PublicProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public bool ShowPhoneToPublic { get; set; }
        public string? ProfileImagePath { get; set; }
        public DateTime RegistrationDate { get; set; }
        public int CreatedSurveysCount { get; set; }
        public int CompletedSurveysCount { get; set; }

        /// <summary>
        /// Получить отображаемое имя
        /// </summary>
        public string GetDisplayName()
        {
            if (!string.IsNullOrWhiteSpace(FirstName) || !string.IsNullOrWhiteSpace(LastName))
            {
                return $"{FirstName} {LastName}".Trim();
            }
            return Email;
        }

        /// <summary>
        /// Получить телефон с учетом настроек приватности
        /// </summary>
        public string? GetDisplayPhone()
        {
            return ShowPhoneToPublic ? PhoneNumber : null;
        }
    }

    /// <summary>
    /// ViewModel для настроек приватности
    /// </summary>
    public class PrivacySettingsViewModel
    {
        [Display(Name = "Показывать телефон всем")]
        public bool ShowPhoneToPublic { get; set; }

        [Display(Name = "Показывать email в публичном профиле")]
        public bool ShowEmailPublic { get; set; }

        [Display(Name = "Разрешить другим пользователям находить меня по email")]
        public bool AllowSearchByEmail { get; set; }

        [Display(Name = "Показывать мои опросы в профиле")]
        public bool ShowSurveysInProfile { get; set; }

        [Display(Name = "Разрешить комментарии к моим опросам")]
        public bool AllowCommentsOnSurveys { get; set; }
    }

    /// <summary>
    /// ViewModel для статистики пользователя
    /// </summary>
    public class UserStatisticsViewModel
    {
        public int TotalSurveysCreated { get; set; }
        public int TotalResponsesReceived { get; set; }
        public int TotalSurveysCompleted { get; set; }
        public DateTime MemberSince { get; set; }
        public int ActiveSurveys { get; set; }
        public int DraftSurveys { get; set; }

        /// <summary>
        /// Получить средний рейтинг опросов
        /// </summary>
        public double GetAverageRating()
        {
            // Можно реализовать логику подсчета среднего рейтинга
            return 0.0;
        }
    }
}

