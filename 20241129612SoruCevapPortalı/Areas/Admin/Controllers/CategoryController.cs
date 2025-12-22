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
        private readonly AppDbContext _context;

        public CategoryController(IRepository<Category> categoryRepo, AppDbContext context)
        {
            _categoryRepo = categoryRepo;
            _context = context;
        }

        public IActionResult Index()
        {
            var categories = _categoryRepo.GetAll();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create() => View();

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
                    var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    return Content($"KRİTİK HATA (SQL): {errorMsg}");
                }
            }
            return View(p);
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
        public IActionResult Delete(int id)
        {
            try
            {
                var category = _categoryRepo.GetById(id);

                if (category == null)
                    return Json(new { success = false, message = "Kategori bulunamadı!" });

                _categoryRepo.Delete(category);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = "Veritabanı Hatası: " + errorMsg });
            }
        }
    }
}