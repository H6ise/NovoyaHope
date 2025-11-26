using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using NovoyaHope.Data;
using NovoyaHope.Models;
using NovoyaHope.Models.ViewModels;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace NovoyaHope.Controllers
{
    [Authorize] // Доступ только для авторизованных пользователей
    public class SurveyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SurveyController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // --- СПИСОК МОИХ ОПРОСОВ ---

        // GET /survey/index
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var mySurveys = await _context.Surveys
                .Where(s => s.CreatorId == userId)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();

            // Маппинг mySurveys в ViewModel для отображения карточек
            return View(mySurveys);
        }

        // --- КОНСТРУКТОР (СОЗДАНИЕ/РЕДАКТИРОВАНИЕ) ---

        // GET /survey/edit/1 (или без ID для нового)
        public async Task<IActionResult> Edit(int? id)
        {
            // 1. Если id == null, создаем пустой опрос и перенаправляем на него.
            if (id == null)
            {
                var newSurvey = new Survey
                {
                    Title = "Новая форма",
                    Description = string.Empty,
                    CreatorId = _userManager.GetUserId(User),
                    CreatedDate = DateTime.UtcNow,
                    IsPublished = false
                };
                _context.Surveys.Add(newSurvey);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Edit), new { id = newSurvey.Id });
            }

            // 2. Загрузка существующего опроса и проверка прав
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (survey == null || survey.CreatorId != _userManager.GetUserId(User))
            {
                return NotFound();
            }

            // Здесь необходим маппинг Survey -> ViewModel для конструктора
            var viewModel = new NovoyaHope.Models.ViewModels.SurveyConstructorViewModel
            {
                Survey = survey,
                Questions = survey.Questions?.OrderBy(q => q.Order).ToList() ?? new System.Collections.Generic.List<NovoyaHope.Models.Question>()
            };

            return View("Constructor", viewModel);
        }

        // --- СОХРАНЕНИЕ / ОБНОВЛЕНИЕ ЧЕРЕЗ AJAX ---

        // POST /api/surveys/save (используется AJAX из constructor.js)
        [HttpPost]
        [Route("api/surveys/save")]
        public async Task<IActionResult> SaveSurvey([FromBody] SaveSurveyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Здесь должна быть сложная логика CRUD для обновления Survey, Question и AnswerOption
            // (обработка model.Id == null для добавления, model.Id != null для обновления)

            await _context.SaveChangesAsync();
            return Ok(new { Success = true, SurveyId = model.Id, Message = "Сохранено" });
        }

        // --- ПУБЛИКАЦИЯ ---

        // POST /survey/publish/1
        [HttpPost]
        public async Task<IActionResult> Publish(int id)
        {
            var survey = await _context.Surveys.FindAsync(id);
            var userId = _userManager.GetUserId(User);

            if (survey == null || survey.CreatorId != userId)
            {
                return Forbid();
            }

            if (!await _context.Questions.AnyAsync(q => q.SurveyId == id))
            {
                return BadRequest(new { message = "Нельзя опубликовать пустой опрос. Добавьте хотя бы один вопрос." });
            }

            survey.IsPublished = true;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Опросник успешно опубликован!" });
        }
    }
}