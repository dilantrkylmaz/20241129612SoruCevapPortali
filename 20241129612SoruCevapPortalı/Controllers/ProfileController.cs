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

            // Zorunlu alanları güncelliyoruz
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;

            // Şifre güncelleme (Artık parametre ismiyle eşleşiyor)
            if (!string.IsNullOrEmpty(newPassword))
            {
                // PasswordHasher ile doğrudan nesne üzerindeki Hash'i güncelliyoruz
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, newPassword);
            }

            // Profil Resmi Yükleme Mantığı (View'da mevcut olduğu için eklendi)
            if (profileImage != null)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(profileImage.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/profiles", fileName);
                using (var stream = new FileStream(path, FileMode.Create)) { await profileImage.CopyToAsync(stream); }
                user.ProfileImageUrl = "/img/profiles/" + fileName;
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Profil bilgileriniz ve şifreniz başarıyla güncellendi.";
            }
            else
            {
                TempData["Error"] = "Güncelleme sırasında bir hata oluştu.";
            }

            return RedirectToAction("Index");
        }
    }
}