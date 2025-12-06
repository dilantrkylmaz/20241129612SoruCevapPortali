using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using _20241129612SoruCevapPortalı.Models;
using _20241129612SoruCevapPortalı.Repositories.Abstract;

namespace _20241129612SoruCevapPortalı.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,MainAdmin")] // Sadece Yöneticiler girebilir
    public class CategoryController : Controller
    {
        // Veritabanı ile konuşacak aracı (Repository)
        private readonly IRepository<Category> _categoryRepo;

        public CategoryController(IRepository<Category> categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        // 1. LİSTELEME: Veritabanından çekip sayfaya gönderir
        public IActionResult Index()
        {
            var categories = _categoryRepo.GetAll();
            return View(categories);
        }

        // 2. EKLEME (Sayfayı Göster)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category p)
        {
            // 1. Önce Model Geçerli mi (Validation) kontrolü
            if (ModelState.IsValid)
            {
                try
                {
                    _categoryRepo.Add(p);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Veritabanı hatası varsa ekrana bas
                    return Content($"KRİTİK HATA (SQL): {ex.Message} \n\n DETAY: {ex.InnerException?.Message}");
                }
            }

            // 2. Model Geçersizse sebebini ekrana bas
            var hataMesajlari = ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList();

            string hataListesi = string.Join("\n- ", hataMesajlari);

            return Content($"DOĞRULAMA (VALIDATION) HATASI VAR! Kayıt yapılmadı.\n\nSebepler:\n- {hataListesi}");
        }
        // 3. GÜNCELLEME (Mevcut veriyi bulup sayfaya gönder)
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _categoryRepo.GetById(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // 3. GÜNCELLEME (Değişiklikleri Kaydet)
        [HttpPost]
        public IActionResult Edit(Category p)
        {
            if (ModelState.IsValid)
            {
                _categoryRepo.Update(p); // Repository üzerinden Güncelle
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
                // Silme işlemi
                _categoryRepo.Delete(category);

                // Başarılı mesajı döndür
                return Json(new { success = true, message = "Kategori başarıyla silindi." });
            }

            // Hata mesajı döndür
            return Json(new { success = false, message = "Kategori bulunamadı!" });
        }
    }
}