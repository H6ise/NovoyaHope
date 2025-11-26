using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using NovoyaHope.Data;
using NovoyaHope.Models;
using NovoyaHope.Models.ViewModels; // Используем SaveSurveyViewModel
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace NovoyaHope.Controllers
{
    // Ограничение доступа только для пользователей с ролью "Administrator"
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // --- УПРАВЛЕНИЕ ШАБЛОНАМИ ---

        [HttpGet]
        public async Task<IActionResult> TemplateManager()
        {
            // Отображение списка существующих шаблонов
            var templates = await _context.SurveyTemplates.ToListAsync();
            // Вернуть ViewModel, содержащую список шаблонов
            return View(templates);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Метод для создания/редактирования шаблона (SaveSurveyViewModel используется для приема данных)
        public async Task<IActionResult> SaveTemplate(SaveSurveyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // При ошибке возвращаем пользователя на страницу с ошибками валидации
                return View("TemplateManager", model);
            }

            // Здесь должна быть логика маппинга SaveSurveyViewModel в сущность SurveyTemplate 
            // и сохранение через _context.SurveyTemplates

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(TemplateManager));
        }

        // --- УПРАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯМИ (Заглушка) ---

        [HttpGet]
        public IActionResult UserManagement()
        {
            return View();
        }
    }
}