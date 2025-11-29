using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NovoyaHope.Models;
using NovoyaHope.Models.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace NovoyaHope.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _environment;

        // ВАЖНО: Роль "User" должна быть создана в DbInitializer!
        private const string DefaultUserRole = "User";

        public AccountController(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _environment = environment;
        }

        // --- Вход (Login) ---

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // Поиск пользователя по Email для входа
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(
                        user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        return RedirectToLocal(returnUrl);
                    }
                }
                ModelState.AddModelError(string.Empty, "Неверная попытка входа или пароль.");
            }
            return View(model);
        }

        // --- Регистрация (Register) ---

        [HttpGet]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["IsRegister"] = true;
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["IsRegister"] = true;
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Назначаем стандартную роль "User"
                    await _userManager.AddToRoleAsync(user, DefaultUserRole);

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToLocal(returnUrl);
                }
                AddErrors(result);
            }
            return View(model);
        }

        // --- Выход (Logout) ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        // --- Профиль пользователя ---

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var model = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ShowPhoneToPublic = user.ShowPhoneToPublic,
                ProfileImagePath = user.ProfileImagePath
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Обновление основной информации
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.ShowPhoneToPublic = model.ShowPhoneToPublic;

            // Обработка загрузки фото профиля
            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                try
                {
                    // Удаление старого фото, если оно есть
                    if (!string.IsNullOrEmpty(user.ProfileImagePath))
                    {
                        NovoyaHope.Helpers.ImageHelper.DeleteProfileImage(user.ProfileImagePath, _environment.WebRootPath);
                    }

                    // Сохранение нового фото с оптимизацией
                    user.ProfileImagePath = await NovoyaHope.Helpers.ImageHelper.SaveProfileImageAsync(
                        model.ProfileImage, 
                        user.Id, 
                        _environment.WebRootPath
                    );
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError(nameof(model.ProfileImage), ex.Message);
                    return View(model);
                }
                catch (Exception)
                {
                    ModelState.AddModelError(nameof(model.ProfileImage), "Ошибка при загрузке изображения");
                    return View(model);
                }
            }

            // Обновление email, если изменился
            if (user.Email != model.Email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    AddErrors(setEmailResult);
                    return View(model);
                }
                var setUserNameResult = await _userManager.SetUserNameAsync(user, model.Email);
                if (!setUserNameResult.Succeeded)
                {
                    AddErrors(setUserNameResult);
                    return View(model);
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Профиль успешно обновлен!";
                return RedirectToAction(nameof(Profile));
            }

            AddErrors(result);
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Проверьте введенные данные";
                return RedirectToAction(nameof(Profile));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Пароль успешно изменен!";
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                TempData["ErrorMessage"] = "Ошибка при изменении пароля";
            }

            return RedirectToAction(nameof(Profile));
        }

        // --- Настройки ---

        [Authorize]
        [HttpGet]
        public IActionResult Settings()
        {
            return View();
        }

        // --- Вспомогательные методы ---

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}