using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using _20241129612SoruCevapPortalı.Models;
using Microsoft.AspNetCore.Identity;

namespace _20241129612SoruCevapPortalı.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,MainAdmin")]
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(UserManager<User> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // "UserId" yerine NameIdentifier kullanıldı
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account", new { area = "" });

            var user = await _userManager.FindByIdAsync(userId);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Index(User p, IFormFile? profileImage, string? newPassword)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                user.FirstName = p.FirstName;
                user.LastName = p.LastName;
                user.Email = p.Email;
                user.PhoneNumber = p.PhoneNumber;

                // ŞİFRE DEĞİŞTİRME MANTIĞI (Düzeltildi)
                if (!string.IsNullOrEmpty(newPassword))
                {
                    // ResetPasswordAsync yerine doğrudan nesne üzerindeki Hash'i güncelliyoruz
                    user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, newPassword);
                }

                if (profileImage != null)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(profileImage.FileName);
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/profiles", fileName);
                    using (var stream = new FileStream(path, FileMode.Create)) { await profileImage.CopyToAsync(stream); }
                    user.ProfileImageUrl = "/img/profiles/" + fileName;
                }

                // Tek bir UpdateAsync ile hem profil hem şifre kaydedilir
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                    TempData["Success"] = "Profil ve şifre başarıyla güncellendi.";
                else
                    TempData["Error"] = "Güncelleme sırasında bir hata oluştu.";
            }
            return RedirectToAction("Index");
        }
    }
}