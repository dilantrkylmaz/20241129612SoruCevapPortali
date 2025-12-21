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
        public async Task<IActionResult> Index(User model, string? newPassword, IFormFile? profileImage)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Temel bilgileri güncelle
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;

            // ŞİFRE GÜNCELLEME (Kesin Çözüm)
            if (!string.IsNullOrEmpty(newPassword))
            {
                // ResetPasswordAsync yerine doğrudan Hash'i güncelliyoruz 
                // Bu sayede UpdateAsync çalışırken eski şifre yeni şifreyi ezmez.
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, newPassword);
            }

            // Profil Resmi İşlemleri
            if (profileImage != null)
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/profiles");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                string fileName = "user_" + user.Id + "_" + Guid.NewGuid() + Path.GetExtension(profileImage.FileName);
                using (var stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }
                user.ProfileImageUrl = "/img/profiles/" + fileName;
            }

            // Veritabanına tek seferde kaydet
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["Success"] = "Profiliniz ve şifreniz başarıyla güncellendi.";
            }
            else
            {
                TempData["Error"] = "Bir hata oluştu: " + string.Join(", ", result.Errors.Select(x => x.Description));
            }

            return RedirectToAction("Index");
        }
    }
}