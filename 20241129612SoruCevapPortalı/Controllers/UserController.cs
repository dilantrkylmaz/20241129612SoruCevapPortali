using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using _20241129612SoruCevapPortalı.Models;

namespace _20241129612SoruCevapPortalı.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _env;

        public UserController(UserManager<User> userManager, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _env = env;
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(User model, IFormFile? profileImage)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;

            if (profileImage != null)
            {
                string path = Path.Combine(_env.WebRootPath, "img/profiles");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                string fileName = Guid.NewGuid() + Path.GetExtension(profileImage.FileName);
                using (var stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }
                user.ProfileImageUrl = "/img/profiles/" + fileName;
            }

            await _userManager.UpdateAsync(user);
            TempData["Message"] = "Profiliniz güncellendi.";
            return RedirectToAction("Profile");
        }
    }
}