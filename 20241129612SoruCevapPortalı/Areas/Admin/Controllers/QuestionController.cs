using _20241129612SoruCevapPortalı.Models;
using _20241129612SoruCevapPortalı.Repositories.Abstract;
using Microsoft.AspNetCore.Mvc;

public class QuestionController : Controller
{
    private readonly IRepository<Question> _repo;
    private readonly IRepository<Category> _catRepo; // Kategoriler için repo ekledik

    public QuestionController(IRepository<Question> repo, IRepository<Category> catRepo)
    {
        _repo = repo;
        _catRepo = catRepo;
    }

    public IActionResult Index(string search, int? categoryId)
    {
        // 1. Cevapları (Answers) da Include ettik ki sayıları doğru gelsin
        var questions = _repo.GetAll(x => x.User, x => x.Category, x => x.Answers);

        // 2. Arama Filtresi
        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            questions = questions.Where(x => x.Title.ToLower().Contains(search)).ToList();
        }

        // 3. Kategori Filtresi
        if (categoryId.HasValue && categoryId.Value > 0)
        {
            questions = questions.Where(x => x.CategoryId == categoryId.Value).ToList();
        }

        // Filtre dropdown'ı için kategorileri View'e gönderiyoruz
        ViewBag.Categories = _catRepo.GetAll();

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