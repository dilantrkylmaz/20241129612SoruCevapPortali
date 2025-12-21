using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _20241129612SoruCevapPortalı.Models;
using _20241129612SoruCevapPortalı.Repositories.Abstract;

namespace _20241129612SoruCevapPortalı.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository<Question> _questionRepo;
        private readonly AppDbContext _context; // Veritabanı context'i eklendi

        public HomeController(IRepository<Question> questionRepo, AppDbContext context)
        {
            _questionRepo = questionRepo;
            _context = context;
        }

        public IActionResult Index(string period = "all")
        {
            // GetAll metoduna lambda ifadeleri ile dahil etme (Include) yapıyoruz
            var questions = _questionRepo.GetAll(x => x.Category, y => y.User, z => z.Answers, l => l.QuestionLikes);

            // Popüler Sorular Mantığı
            DateTime filterDate = period switch
            {
                "day" => DateTime.Now.AddDays(-1),
                "week" => DateTime.Now.AddDays(-7),
                "month" => DateTime.Now.AddMonths(-1),
                "year" => DateTime.Now.AddYears(-1),
                _ => DateTime.MinValue
            };

            ViewBag.PopularQuestions = questions
                .Where(x => x.CreatedDate >= filterDate)
                .OrderByDescending(x => x.QuestionLikes?.Count ?? 0)
                .Take(5)
                .ToList();

            return View(questions.OrderByDescending(x => x.CreatedDate).ToList());
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}