using Microsoft.AspNetCore.Mvc;
using _20241129612SoruCevapPortalı.Models;
using _20241129612SoruCevapPortalı.Repositories.Abstract;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace _20241129612SoruCevapPortalı.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository<Question> _questionRepo;
        private readonly IRepository<Category> _categoryRepo; // Kategori repo eklendi
        private readonly AppDbContext _context;

        public HomeController(IRepository<Question> questionRepo, IRepository<Category> categoryRepo, AppDbContext context)
        {
            _questionRepo = questionRepo;
            _categoryRepo = categoryRepo;
            _context = context;
        }

        public IActionResult Index(string search, int? categoryId, string sortBy, string period = "all")
        {
            // Tüm soruları ilişkili verileriyle çekiyoruz
            var questions = _questionRepo.GetAll(x => x.Category, y => y.User, z => z.Answers, l => l.QuestionLikes).AsEnumerable();

            // 1. Arama Filtresi (Başlık veya İçerik)
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                questions = questions.Where(q => q.Title.ToLower().Contains(search) || q.Content.ToLower().Contains(search));
            }

            // 2. Kategori Filtresi
            if (categoryId.HasValue && categoryId > 0)
            {
                questions = questions.Where(q => q.CategoryId == categoryId);
            }

            // 3. Sıralama Mantığı
            questions = sortBy switch
            {
                "oldest" => questions.OrderBy(q => q.CreatedDate),
                "alpha" => questions.OrderBy(q => q.Title),
                "alpha-desc" => questions.OrderByDescending(q => q.Title),
                _ => questions.OrderByDescending(q => q.CreatedDate) // Varsayılan: En Yeni
            };

            // Sidebar: Popüler Sorular Mantığı (Mevcut yapıyı koruyoruz)
            DateTime filterDate = period switch
            {
                "day" => DateTime.Now.AddDays(-1),
                "week" => DateTime.Now.AddDays(-7),
                "month" => DateTime.Now.AddMonths(-1),
                "year" => DateTime.Now.AddYears(-1),
                _ => DateTime.MinValue
            };

            var allQuestionsForPopular = _questionRepo.GetAll(x => x.QuestionLikes);
            ViewBag.PopularQuestions = allQuestionsForPopular
                .Where(x => x.CreatedDate >= filterDate)
                .OrderByDescending(x => x.QuestionLikes?.Count ?? 0)
                .Take(5).ToList();

            // View tarafı için kategorileri hazırlıyoruz
            ViewBag.Categories = new SelectList(_categoryRepo.GetAll(), "Id", "Name", categoryId);

            // Mevcut filtre değerlerini View'a geri gönderiyoruz (inputlarda kalması için)
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSort = sortBy;

            return View(questions.ToList());
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}