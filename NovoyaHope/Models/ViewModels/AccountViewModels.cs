using System.ComponentModel.DataAnnotations;

namespace NovoyaHope.Models.ViewModels.AccountViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введите email.")]
        [EmailAddress(ErrorMessage = "Неверный формат email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Введите пароль.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }
}