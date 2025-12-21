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
        public async Task<IActionResult> Index(User p, IFormFile? profileImage)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                // Boş gelirse eski veriyi koru (NULL hatasını bu engeller)
                user.FirstName = !string.IsNullOrEmpty(p.FirstName) ? p.FirstName : user.FirstName;
                user.LastName = !string.IsNullOrEmpty(p.LastName) ? p.LastName : user.LastName;
                user.PhoneNumber = p.PhoneNumber;

                // Şifre güncelleme mantığı
                if (!string.IsNullOrEmpty(p.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, token, p.Password);
                }

                if (profileImage != null)
                {
                    string newImageName = "user_" + Guid.NewGuid() + Path.GetExtension(profileImage.FileName);
                    string path = Path.Combine(_webHostEnvironment.WebRootPath, "img/profiles", newImageName);
                    using (var stream = new FileStream(path, FileMode.Create)) { await profileImage.CopyToAsync(stream); }
                    user.ProfileImageUrl = "/img/profiles/" + newImageName;
                }

                await _userManager.UpdateAsync(user);
                TempData["Success"] = "Profil başarıyla güncellendi.";
            }
            return RedirectToAction("Index");
        }
    }
}