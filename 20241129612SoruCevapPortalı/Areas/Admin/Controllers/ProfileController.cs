using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using _20241129612SoruCevapPortalı.Models;
using _20241129612SoruCevapPortalı.Repositories.Abstract;
using _20241129612SoruCevapPortalı.Helpers;

namespace _20241129612SoruCevapPortalı.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,MainAdmin")]
    public class ProfileController : Controller
    {
        private readonly IRepository<User> _userRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(IRepository<User> userRepo, IWebHostEnvironment webHostEnvironment)
        {
            _userRepo = userRepo;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userIdStr = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account", new { area = "" });

            int userId = int.Parse(userIdStr);
            var user = _userRepo.GetById(userId);

            return View(user);
        }

        [HttpPost]
        public IActionResult Index(User p, IFormFile? ProfileImage)
        {
            var userIdStr = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account", new { area = "" });

            var userId = int.Parse(userIdStr);
            var user = _userRepo.GetById(userId);

            if (user != null)
            {
                if (!string.IsNullOrEmpty(p.FirstName)) user.FirstName = p.FirstName;
                if (!string.IsNullOrEmpty(p.LastName)) user.LastName = p.LastName;
                if (!string.IsNullOrEmpty(p.Email)) user.Email = p.Email;
                if (!string.IsNullOrEmpty(p.PhoneNumber)) user.PhoneNumber = p.PhoneNumber;

              
                if (string.IsNullOrEmpty(user.FirstName)) user.FirstName = "Sistem";
                if (string.IsNullOrEmpty(user.LastName)) user.LastName = "Yöneticisi";
                if (string.IsNullOrEmpty(user.Email)) user.Email = "admin@portal.com";
                if (string.IsNullOrEmpty(user.PhoneNumber)) user.PhoneNumber = "5555555555";

                if (!string.IsNullOrEmpty(p.Password))
                {
                    user.Password = SecurityHelper.HashPassword(p.Password);
                }

               
                if (ProfileImage != null)
                {
                    string extension = Path.GetExtension(ProfileImage.FileName);
                    string newImageName = "user_" + userId + "_" + Guid.NewGuid() + extension;
                    string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "profiles");

                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    using (var stream = new FileStream(Path.Combine(folderPath, newImageName), FileMode.Create))
                    {
                        ProfileImage.CopyTo(stream);
                    }
                    user.ProfileImageUrl = "/img/profiles/" + newImageName;
                }

                _userRepo.Update(user);
                TempData["Success"] = "Profil başarıyla güncellendi.";
            }

            return RedirectToAction("Index");
        }
    }
}