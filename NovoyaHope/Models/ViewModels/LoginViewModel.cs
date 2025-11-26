using System.ComponentModel.DataAnnotations;

namespace NovoyaHope.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введите ваш Email")]
        [EmailAddress(ErrorMessage = "Некорректный формат Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Введите ваш пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Display(Name = "Запомнить меня?")]
        public bool RememberMe { get; set; }
    }
}