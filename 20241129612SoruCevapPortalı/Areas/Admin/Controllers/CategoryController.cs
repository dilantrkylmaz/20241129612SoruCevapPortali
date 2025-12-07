using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using _20241129612SoruCevapPortalı.Models;
using _20241129612SoruCevapPortalı.Repositories.Abstract;

namespace _20241129612SoruCevapPortalı.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,MainAdmin")] 
    public class CategoryController : Controller
    {
        private readonly IRepository<Category> _categoryRepo;

        public CategoryController(IRepository<Category> categoryRepo)
        {
            _categoryRepo = categoryRepo;
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

        [HttpPost]
        public IActionResult DeleteAjax(int id)
        {
            var category = _categoryRepo.GetById(id);
            if (category != null)
            {
                _categoryRepo.Delete(category);

                return Json(new { success = true, message = "Kategori başarıyla silindi." });
            }

            return Json(new { success = false, message = "Kategori bulunamadı!" });
        }
    }
}