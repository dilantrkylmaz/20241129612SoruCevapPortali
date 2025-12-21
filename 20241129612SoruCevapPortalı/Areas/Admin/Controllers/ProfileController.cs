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
                user.FirstName = p.FirstName;
                user.LastName = p.LastName;
                user.Email = p.Email;
                user.PhoneNumber = p.PhoneNumber;

                if (!string.IsNullOrEmpty(p.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, token, p.Password);
                }

                if (profileImage != null)
                {
                    string extension = Path.GetExtension(profileImage.FileName);
                    string newImageName = "user_" + user.Id + "_" + Guid.NewGuid() + extension;
                    string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "profiles");

                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    using (var stream = new FileStream(Path.Combine(folderPath, newImageName), FileMode.Create))
                    {
                        await profileImage.CopyToAsync(stream);
                    }
                    user.ProfileImageUrl = "/img/profiles/" + newImageName;
                }

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Profil başarıyla güncellendi.";
                }
            }

            return RedirectToAction("Index");
        }
    }
}