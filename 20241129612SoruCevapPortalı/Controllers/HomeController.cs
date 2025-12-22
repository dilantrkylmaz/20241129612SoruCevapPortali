using Microsoft.AspNetCore.Mvc;
using _20241129612SoruCevapPortalı.Models;
using _20241129612SoruCevapPortalı.Repositories.Abstract;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace _20241129612SoruCevapPortalı.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository<Question> _questionRepo;
        private readonly IRepository<Category> _categoryRepo;
        private readonly AppDbContext _context;

        public HomeController(IRepository<Question> questionRepo, IRepository<Category> categoryRepo, AppDbContext context)
        {
            _questionRepo = questionRepo;
            _categoryRepo = categoryRepo;
            _context = context;
        }

        public IActionResult Index(string search, int? categoryId, string sortBy, string period = "all", int page = 1)
        {
            int pageSize = 5;

            IEnumerable<Question> query = _questionRepo.GetAll(x => x.Category, y => y.User, z => z.Answers, l => l.QuestionLikes).AsEnumerable();

            if (!string.IsNullOrEmpty(search))
            {
                string s = search.ToLower();
                query = query.Where(q => q.Title.ToLower().Contains(s) || q.Content.ToLower().Contains(s));
            }

            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(q => q.CategoryId == categoryId);
            }

            query = sortBy switch
            {
                "oldest" => query.OrderBy(q => q.CreatedDate),
                "alpha" => query.OrderBy(q => q.Title),
                "alpha-desc" => query.OrderByDescending(q => q.Title),
                _ => query.OrderByDescending(q => q.CreatedDate)
            };

            int totalQuestions = query.Count();

            var paginatedQuestions = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            DateTime filterDate = period switch
            {
                "day" => DateTime.Now.AddDays(-1),
                "week" => DateTime.Now.AddDays(-7),
                "month" => DateTime.Now.AddMonths(-1),
                _ => DateTime.MinValue
            };

            ViewBag.PopularQuestions = _questionRepo.GetAll(x => x.QuestionLikes)
                .Where(x => x.CreatedDate >= filterDate)
                .OrderByDescending(x => x.QuestionLikes?.Count ?? 0)
                .Take(5).ToList();

            ViewBag.Categories = new SelectList(_categoryRepo.GetAll().ToList(), "Id", "Name", categoryId);
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSort = sortBy;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalQuestions / pageSize);

            return View(paginatedQuestions);
        }
    }
}