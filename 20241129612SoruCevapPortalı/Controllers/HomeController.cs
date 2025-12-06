using Microsoft.AspNetCore.Mvc;
using _20241129612SoruCevapPortalı.Models;
using _20241129612SoruCevapPortalı.Repositories.Abstract;

namespace _20241129612SoruCevapPortalı.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository<Question> _questionRepo;

        public HomeController(IRepository<Question> questionRepo)
        {
            _questionRepo = questionRepo;
        }

        public IActionResult Index()
        {
            // Soruları; Kategorisi, Kullanıcısı ve Cevaplarıyla birlikte getir
            var questions = _questionRepo.GetAll(x => x.Category, y => y.User, z => z.Answers);

            // En yeniden eskiye doğru sırala
            return View(questions.OrderByDescending(x => x.CreatedDate).ToList());
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}