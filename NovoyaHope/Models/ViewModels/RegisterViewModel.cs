using System.ComponentModel.DataAnnotations;

namespace NovoyaHope.Models.ViewModels.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введите email.")]
        [EmailAddress(ErrorMessage = "Неверный формат email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Введите пароль.")]
        [StringLength(100, ErrorMessage = "Пароль должен содержать минимум {2} символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтвердите пароль")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают.")]
        public string ConfirmPassword { get; set; }
    }
}