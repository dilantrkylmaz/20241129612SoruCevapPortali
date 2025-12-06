using _20241129612SoruCevapPortalı.Models;
using _20241129612SoruCevapPortalı.Repositories.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _20241129612SoruCevapPortalı.Areas.Admin.Controllers
{
    [Area("Admin")] // <--- BU SATIR EKSİKTİ, O YÜZDEN SAYFA AÇILMIYOR
    [Authorize(Roles = "Admin,MainAdmin")] // Sadece yöneticiler girebilsin
    public class QuestionController : Controller
    {
        private readonly IRepository<Question> _repo;
        private readonly IRepository<Category> _catRepo; // Kategoriler için repo ekledik

        public QuestionController(IRepository<Question> repo, IRepository<Category> catRepo)
        {
            _repo = repo;
            _catRepo = catRepo;
        }

        [HttpGet]
        public IActionResult Index(string search, int? categoryId, string searchUser)
        {
            // 1. Verileri ilişkileriyle (User, Category, Answers) çek
            var questions = _repo.GetAll(x => x.User, x => x.Category, x => x.Answers);

            // 2. Başlığa Göre Filtrele
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                questions = questions.Where(x => x.Title.ToLower().Contains(search)).ToList();
            }

            // 3. Kategoriye Göre Filtrele
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                questions = questions.Where(x => x.CategoryId == categoryId.Value).ToList();
            }

            // 4. Soran Kişiye Göre Filtrele (YENİ EKLENDİ)
            if (!string.IsNullOrEmpty(searchUser))
            {
                searchUser = searchUser.ToLower();
                questions = questions.Where(x =>
                    x.User != null &&
                    (x.User.Username.ToLower().Contains(searchUser) ||
                     x.User.FirstName.ToLower().Contains(searchUser) ||
                     x.User.LastName.ToLower().Contains(searchUser))
                ).ToList();
            }

            // Filtre değerlerini View'e geri gönder (Kutular boşalmasın)
            ViewBag.Categories = _catRepo.GetAll();
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.SearchUser = searchUser;

            return View(questions);
        }

        // SORUYU SİL
        public IActionResult Delete(int id)
        {
            var question = _repo.GetById(id);
            if (question != null)
            {
                _repo.Delete(question);
            }
            return RedirectToAction("Index");
        }
    }
}