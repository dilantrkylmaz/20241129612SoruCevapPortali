using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using _20241129612SoruCevapPortalı.Models;
using _20241129612SoruCevapPortalı.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace _20241129612SoruCevapPortalı.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,MainAdmin")]
    public class CategoryController : Controller
    {
        private readonly IRepository<Category> _categoryRepo;
        private readonly AppDbContext _context; // DÜZELTME: Veritabanı context'i eklendi

        public CategoryController(IRepository<Category> categoryRepo, AppDbContext context)
        {
            _categoryRepo = categoryRepo;
            _context = context; // DÜZELTME: Context constructor üzerinden enjekte edildi
        }

        public IActionResult Index()
        {
            var categories = _categoryRepo.GetAll();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category p)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _categoryRepo.Add(p);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    return Content($"KRİTİK HATA (SQL): {ex.Message} \n\n DETAY: {ex.InnerException?.Message}");
                }
            }

            var hataMesajlari = ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList();

            string hataListesi = string.Join("\n- ", hataMesajlari);
            return Content($"DOĞRULAMA (VALIDATION) HATASI VAR! Kayıt yapılmadı.\n\nSebepler:\n- {hataListesi}");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _categoryRepo.GetById(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category p)
        {
            if (ModelState.IsValid)
            {
                _categoryRepo.Update(p);
                return RedirectToAction("Index");
            }
            return View(p);
        }

        [Authorize(Roles = "Admin,MainAdmin")]
        public IActionResult Delete(int id)
        {
            // Kategoriye bağlı soru var mı kontrolü (Güvenlik Maddesi)
            // Artık _context nesnesi yukarıda tanımlandığı için bu satır hata vermeyecektir.
            bool hasQuestions = _context.Questions.Any(q => q.CategoryId == id);

            if (hasQuestions)
            {
                TempData["Error"] = "Bu kategoriye ait sorular olduğu için silemezsiniz! Önce soruları başka kategoriye taşıyın veya silin.";
                return RedirectToAction("Index");
            }

            var category = _categoryRepo.GetById(id);
            if (category != null)
            {
                _categoryRepo.Delete(category);
                TempData["Success"] = "Kategori başarıyla silindi.";
            }
            return RedirectToAction("Index");
        }
    }
}