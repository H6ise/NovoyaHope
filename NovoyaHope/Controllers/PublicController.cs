using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovoyaHope.Data;
using NovoyaHope.Models;
using NovoyaHope.Models.ViewModels.PassSurveyViewModels;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using NovoyaHope.Models.DataModels;

namespace NovoyaHope.Controllers
{
    [AllowAnonymous]
    public class PublicController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PublicController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET /public/viewsurvey/1
        public async Task<IActionResult> ViewSurvey(int id)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions.OrderBy(q => q.Order))
                    .ThenInclude(q => q.AnswerOptions.OrderBy(o => o.Order))
                .FirstOrDefaultAsync(s => s.Id == id && s.IsPublished);

            if (survey == null)
            {
                return NotFound("Опрос не найден или не опубликован.");
            }

            // Здесь нужен маппинг Survey -> PassSurveyViewModel
            var viewModel = new PassSurveyViewModel
            {
                Id = survey.Id,
                Title = survey.Title,
                Description = survey.Description,
                IsAnonymous = survey.IsAnonymous,
                // ... (Маппинг вопросов и опций)
            };

            return View("Pass", viewModel);
        }

        // POST /public/submitresponse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitResponse(SubmitResponseViewModel model)
        {
            // ... (Логика обработки ответов из предыдущего шага) ...

            // 1. Проверка существования и статуса опроса
            var survey = await _context.Surveys.FindAsync(model.SurveyId);

            if (survey == null || !survey.IsPublished) return NotFound();

            // 2. Создание записи SurveyResponse
            var responseEntry = new SurveyResponse
            {
                SurveyId = model.SurveyId,
                SubmissionDate = DateTime.UtcNow,
                UserId = survey.IsAnonymous ? null : User.FindFirstValue(ClaimTypes.NameIdentifier),
                UserAnswers = new List<UserAnswer>()
            };

            // 3. Обработка всех ответов и сохранение...

            _context.SurveyResponses.Add(responseEntry);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ThankYou));
        }

        public IActionResult ThankYou()
        {
            return View();
        }
    }
}