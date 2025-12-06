using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using _20241129612SoruCevapPortalı.Models;
using System.Linq;

namespace _20241129612SoruCevapPortalı.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,MainAdmin")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Veritabanından sayıları çekip ViewBag ile sayfaya taşıyoruz
            ViewBag.UserCount = _context.Users.Count();
            ViewBag.QuestionCount = _context.Questions.Count();
            ViewBag.CategoryCount = _context.Categories.Count();
            ViewBag.AnswerCount = _context.Answers.Count();

            return View();
        }
    }
}