using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using _20241129612SoruCevapPortalı.Models;
using _20241129612SoruCevapPortalı.Repositories.Abstract;

namespace _20241129612SoruCevapPortalı.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,MainAdmin")]
    public class ProfileController : Controller
    {
        private readonly IRepository<User> _userRepo;
        private readonly IWebHostEnvironment _webHostEnvironment; // Dosya kaydetmek için lazım

        public ProfileController(IRepository<User> userRepo, IWebHostEnvironment webHostEnvironment)
        {
            _userRepo = userRepo;
            _webHostEnvironment = webHostEnvironment;
        }

        // PROFİLİ GÖSTER
        [HttpPost]
        public IActionResult Index(User p, IFormFile? ProfileImage)
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));
            var user = _userRepo.GetById(userId);

            if (user != null)
            {
                // 1. AD VE SOYAD GÜNCELLEME (Burası eksikti, ekledik)
                user.FirstName = p.FirstName;
                user.LastName = p.LastName;

                // 2. Diğer Bilgiler
                user.Username = p.Username;
                user.Email = p.Email;
                user.PhoneNumber = p.PhoneNumber;

                // 3. Şifre Değişikliği (Boş bırakırsa eski şifre kalır)
                if (!string.IsNullOrEmpty(p.Password))
                {
                    user.Password = p.Password;
                }

                // 4. Resim Yükleme İşlemi (Aynen kalsın)
                if (ProfileImage != null)
                {
                    string extension = Path.GetExtension(ProfileImage.FileName);
                    string newImageName = "user_" + userId + "_" + Guid.NewGuid() + extension;
                    string location = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/profiles/", newImageName);

                    using (var stream = new FileStream(location, FileMode.Create))
                    {
                        ProfileImage.CopyTo(stream);
                    }
                    user.ProfileImageUrl = "/img/profiles/" + newImageName;
                }

                _userRepo.Update(user);
                TempData["Success"] = "Profil bilgileriniz güncellendi.";
            }

            return RedirectToAction("Index");
        }
    }
}