using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using NovoyaHope.Data;
using NovoyaHope.Models;
using NovoyaHope.Models.ViewModels;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

namespace NovoyaHope.Controllers
{
    // Ограничение доступа только для пользователей с ролью "Administrator"
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // --- ГЛАВНАЯ СТРАНИЦА АДМИН-ПАНЕЛИ (Dashboard) ---

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var stats = new AdminDashboardViewModel
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalSurveys = await _context.Surveys.CountAsync(),
                TotalTemplates = await _context.SurveyTemplates.CountAsync(),
                TotalResponses = await _context.SurveyResponses.CountAsync(),
                PublishedSurveys = await _context.Surveys.CountAsync(s => s.IsPublished),
                RecentSurveys = await _context.Surveys
                    .Include(s => s.Creator)
                    .OrderByDescending(s => s.CreatedDate)
                    .Take(5)
                    .Select(s => new SurveyListItemViewModel
                    {
                        Id = s.Id,
                        Title = s.Title,
                        CreatorName = s.Creator != null ? (s.Creator.FirstName + " " + s.Creator.LastName).Trim() : s.CreatorId,
                        CreatedDate = s.CreatedDate,
                        IsPublished = s.IsPublished,
                        ResponseCount = s.Responses != null ? s.Responses.Count : 0
                    })
                    .ToListAsync(),
                RecentUsers = await _userManager.Users
                    .OrderByDescending(u => u.Id)
                    .Take(5)
                    .Select(u => new UserListItemViewModel
                    {
                        Id = u.Id,
                        Email = u.Email,
                        UserName = u.UserName,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        CreatedSurveysCount = u.CreatedSurveys != null ? u.CreatedSurveys.Count : 0
                    })
                    .ToListAsync()
            };

            return View(stats);
        }

        // --- УПРАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯМИ ---

        [HttpGet]
        public async Task<IActionResult> UserManagement(string searchTerm = "", int page = 1, int pageSize = 20)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => 
                    u.Email.Contains(searchTerm) || 
                    u.UserName.Contains(searchTerm) ||
                    (u.FirstName != null && u.FirstName.Contains(searchTerm)) ||
                    (u.LastName != null && u.LastName.Contains(searchTerm)));
            }

            var totalUsers = await query.CountAsync();
            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userViewModels = new List<UserManagementViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserManagementViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = roles.ToList(),
                    CreatedSurveysCount = await _context.Surveys.CountAsync(s => s.CreatorId == user.Id),
                    ResponsesCount = await _context.SurveyResponses.CountAsync(r => r.UserId == user.Id)
                });
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
            ViewBag.TotalUsers = totalUsers;

            return View(userViewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Проверяем, не является ли это последним администратором
            var adminUsers = await _userManager.GetUsersInRoleAsync("Administrator");
            if (adminUsers.Count == 1 && adminUsers.First().Id == id)
            {
                TempData["Error"] = "Нельзя удалить последнего администратора.";
                return RedirectToAction(nameof(UserManagement));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Пользователь успешно удален.";
            }
            else
            {
                TempData["Error"] = "Ошибка при удалении пользователя.";
            }

            return RedirectToAction(nameof(UserManagement));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserRole(string userId, string roleName)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleName))
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var isInRole = await _userManager.IsInRoleAsync(user, roleName);
            
            if (isInRole)
            {
                // Проверяем, не является ли это последним администратором
                if (roleName == "Administrator")
                {
                    var adminUsers = await _userManager.GetUsersInRoleAsync("Administrator");
                    if (adminUsers.Count == 1 && adminUsers.First().Id == userId)
                    {
                        TempData["Error"] = "Нельзя удалить роль администратора у последнего администратора.";
                        return RedirectToAction(nameof(UserManagement));
                    }
                }
                await _userManager.RemoveFromRoleAsync(user, roleName);
                TempData["Success"] = $"Роль '{roleName}' успешно удалена у пользователя.";
            }
            else
            {
                await _userManager.AddToRoleAsync(user, roleName);
                TempData["Success"] = $"Роль '{roleName}' успешно добавлена пользователю.";
            }

            return RedirectToAction(nameof(UserManagement));
        }

        // --- УПРАВЛЕНИЕ ОПРОСАМИ ---

        [HttpGet]
        public async Task<IActionResult> Surveys(string searchTerm = "", int page = 1, int pageSize = 20)
        {
            var query = _context.Surveys
                .Include(s => s.Creator)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => 
                    s.Title.Contains(searchTerm) || 
                    s.Description.Contains(searchTerm));
            }

            var totalSurveys = await query.CountAsync();
            var surveys = await query
                .OrderByDescending(s => s.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SurveyManagementViewModel
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    CreatorName = s.Creator != null ? (s.Creator.FirstName + " " + s.Creator.LastName).Trim() : s.CreatorId,
                    CreatorEmail = s.Creator != null ? s.Creator.Email : "",
                    CreatedDate = s.CreatedDate,
                    IsPublished = s.IsPublished,
                    Type = s.Type,
                    ResponseCount = s.Responses != null ? s.Responses.Count : 0,
                    QuestionCount = s.Questions != null ? s.Questions.Count : 0
                })
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalSurveys / (double)pageSize);
            ViewBag.TotalSurveys = totalSurveys;

            return View(surveys);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSurvey(int id)
        {
            var survey = await _context.Surveys.FindAsync(id);
            if (survey == null)
            {
                return NotFound();
            }

            _context.Surveys.Remove(survey);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Опрос успешно удален.";
            return RedirectToAction(nameof(Surveys));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSurveyPublish(int id)
        {
            var survey = await _context.Surveys.FindAsync(id);
            if (survey == null)
            {
                return NotFound();
            }

            survey.IsPublished = !survey.IsPublished;
            await _context.SaveChangesAsync();

            TempData["Success"] = survey.IsPublished 
                ? "Опрос опубликован." 
                : "Опрос снят с публикации.";

            return RedirectToAction(nameof(Surveys));
        }

        // --- УПРАВЛЕНИЕ ШАБЛОНАМИ ---

        [HttpGet]
        public async Task<IActionResult> TemplateManager(string searchTerm = "", int page = 1, int pageSize = 20)
        {
            var query = _context.SurveyTemplates.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(t => 
                    t.Title.Contains(searchTerm) || 
                    t.Description.Contains(searchTerm));
            }

            var totalTemplates = await query.CountAsync();
            var templates = await query
                .OrderByDescending(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TemplateManagementViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Type = t.Type,
                    QuestionCount = t.Questions != null ? t.Questions.Count : 0
                })
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalTemplates / (double)pageSize);
            ViewBag.TotalTemplates = totalTemplates;

            return View(templates);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            var template = await _context.SurveyTemplates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            _context.SurveyTemplates.Remove(template);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Шаблон успешно удален.";
            return RedirectToAction(nameof(TemplateManager));
        }
    }
}