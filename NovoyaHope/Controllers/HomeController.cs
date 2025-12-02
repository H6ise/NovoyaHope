using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovoyaHope.Models;
using NovoyaHope.Data;
using NovoyaHope.Models.ViewModels; 
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
            // ��������� ��������� 5 �������� ��� ����������� �� ������� ��������
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
            // ������������, ��� ErrorViewModel ����������
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // --- Дополнительные страницы для навигации ---

        public IActionResult About()
        {
            ViewData["Title"] = "О нас";
            return View();
        }

        public IActionResult Features()
        {
            ViewData["Title"] = "Возможности";
            return View();
        }

        public IActionResult Pricing()
        {
            ViewData["Title"] = "Тарифы";
            return View();
        }

        public IActionResult Help()
        {
            ViewData["Title"] = "Помощь";
            return View();
        }

        public IActionResult Documentation()
        {
            ViewData["Title"] = "Документация";
            return View();
        }

        public IActionResult Blog()
        {
            ViewData["Title"] = "Блог";
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Title"] = "Контакты";
            return View();
        }

        public IActionResult Privacy()
        {
            ViewData["Title"] = "Политика конфиденциальности";
            return View();
        }

        public IActionResult Terms()
        {
            ViewData["Title"] = "Условия использования";
            return View();
        }

        public IActionResult Cookies()
        {
            ViewData["Title"] = "Cookie";
            return View();
        }
    }
}