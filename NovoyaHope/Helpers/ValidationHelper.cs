using System.Text.RegularExpressions;

namespace NovoyaHope.Helpers
{
    /// <summary>
    /// Вспомогательный класс для валидации данных
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Проверка валидности email
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Проверка валидности телефона
        /// </summary>
        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Удаляем все нецифровые символы кроме +
            var cleanPhone = Regex.Replace(phone, @"[^\d+]", "");
            
            // Проверяем длину (от 10 до 15 цифр)
            var digitsOnly = Regex.Replace(cleanPhone, @"[^\d]", "");
            return digitsOnly.Length >= 10 && digitsOnly.Length <= 15;
        }

        /// <summary>
        /// Форматирование телефона
        /// </summary>
        public static string FormatPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return phone;

            // Удаляем все нецифровые символы кроме +
            var cleanPhone = Regex.Replace(phone, @"[^\d+]", "");
            
            // Форматируем российские номера
            if (cleanPhone.StartsWith("8") && cleanPhone.Length == 11)
            {
                cleanPhone = "+7" + cleanPhone.Substring(1);
            }
            else if (cleanPhone.StartsWith("7") && cleanPhone.Length == 11)
            {
                cleanPhone = "+" + cleanPhone;
            }

            // Форматируем: +7 (999) 123-45-67
            if (cleanPhone.StartsWith("+7") && cleanPhone.Length == 12)
            {
                return $"+7 ({cleanPhone.Substring(2, 3)}) {cleanPhone.Substring(5, 3)}-{cleanPhone.Substring(8, 2)}-{cleanPhone.Substring(10, 2)}";
            }

            return phone;
        }

        /// <summary>
        /// Проверка надежности пароля
        /// </summary>
        public static (bool IsValid, string Message) ValidatePasswordStrength(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Пароль не может быть пустым");

            if (password.Length < 6)
                return (false, "Пароль должен содержать минимум 6 символов");

            if (!Regex.IsMatch(password, @"[a-z]"))
                return (false, "Пароль должен содержать минимум одну строчную букву");

            if (!Regex.IsMatch(password, @"[A-Z]"))
                return (false, "Пароль должен содержать минимум одну заглавную букву");

            if (!Regex.IsMatch(password, @"\d"))
                return (false, "Пароль должен содержать минимум одну цифру");

            return (true, "Пароль соответствует требованиям");
        }

        /// <summary>
        /// Проверка имени/фамилии
        /// </summary>
        public static bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return true; // Имя необязательно

            if (name.Length > 50)
                return false;

            // Только буквы, пробелы и дефисы
            return Regex.IsMatch(name, @"^[а-яА-ЯёЁa-zA-Z\s-]+$");
        }

        /// <summary>
        /// Санитизация строки (удаление опасных символов)
        /// </summary>
        public static string SanitizeString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // Удаляем HTML теги
            var withoutHtml = Regex.Replace(input, @"<[^>]*>", string.Empty);
            
            // Удаляем потенциально опасные символы
            var sanitized = Regex.Replace(withoutHtml, @"[<>""']", string.Empty);
            
            return sanitized.Trim();
        }

        /// <summary>
        /// Проверка длины строки
        /// </summary>
        public static bool IsValidLength(string input, int minLength, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(input))
                return minLength == 0;

            return input.Length >= minLength && input.Length <= maxLength;
        }

        /// <summary>
        /// Проверка наличия недопустимых символов
        /// </summary>
        public static bool ContainsInvalidCharacters(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Проверяем на наличие управляющих символов и специальных символов
            return Regex.IsMatch(input, @"[\x00-\x1F\x7F]");
        }
    }
}

