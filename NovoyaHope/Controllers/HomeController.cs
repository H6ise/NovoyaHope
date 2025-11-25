using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovoyaHope.Models;
using NovoyaHope.Data;
using NovoyaHope.Models.ViewModels; // Предполагаем, что ErrorViewModel находится здесь
using NovoyaHope.Models.ViewModels.SurveyViewModels; // Нужна для HomeIndexViewModel
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NovoyaHope.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Получение последних 5 шаблонов для отображения на главной странице
            var templates = await _context.SurveyTemplates
                                          .Take(5)
                                          .Select(t => new TemplateDisplayViewModel
                                          {
                                              Id = t.Id,
                                              Title = t.Title,
                                              TypeDescription = t.Type.ToString(),
                                              PreviewBackgroundColor = "#f1f3f4"
                                          })
                                          .ToListAsync();

            var viewModel = new HomeIndexViewModel
            {
                TemplatePreviews = templates
            };

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Предполагаем, что ErrorViewModel определена
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}