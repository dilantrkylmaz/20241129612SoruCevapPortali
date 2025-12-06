using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using _20241129612SoruCevapPortalı.Models;
using _20241129612SoruCevapPortalı.Repositories.Abstract;

namespace _20241129612SoruCevapPortalı.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Sadece Yöneticiler girebilir
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
            if (ModelState.IsValid)
            {
                _categoryRepo.Add(p);
                return RedirectToAction("Index");
            }

            // --- HATAYI BULMAK İÇİN BU KODU EKLE ---
            var hatalar = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var hata in hatalar)
            {
                // Visual Studio'nun altındaki Output penceresine hatayı yazar
                System.Diagnostics.Debug.WriteLine("--------------------------------");
                System.Diagnostics.Debug.WriteLine("VALIDATION HATASI: " + hata.ErrorMessage);
                System.Diagnostics.Debug.WriteLine("--------------------------------");
            }
            // ----------------------------------------

            return View(p);
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

        // 4. SİLME
        public IActionResult Delete(int id)
        {
            var category = _categoryRepo.GetById(id);
            if (category != null)
            {
                _categoryRepo.Delete(category); // Repository üzerinden Sil
            }
            return RedirectToAction("Index");
        }
    }
}