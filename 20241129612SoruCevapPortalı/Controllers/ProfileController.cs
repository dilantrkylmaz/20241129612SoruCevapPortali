using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using _20241129612SoruCevapPortalı.Models;

namespace _20241129612SoruCevapPortalı.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;

        public ProfileController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Mevcut giriş yapmış kullanıcıyı Identity üzerinden güvenli şekilde getirir
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Index(User model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Ad ve Soyad zorunlu alanlardır, boş gelirse hatayı önlemek için kontrol ediyoruz
            if (string.IsNullOrEmpty(model.FirstName) || string.IsNullOrEmpty(model.LastName))
            {
                TempData["Error"] = "Ad ve Soyad alanları boş bırakılamaz!";
                return View(user);
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Profiliniz güncellendi.";
            }
            return RedirectToAction("Index");
        }
    }
}