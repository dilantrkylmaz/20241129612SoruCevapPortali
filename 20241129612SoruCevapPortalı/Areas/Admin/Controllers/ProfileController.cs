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
        public IActionResult Index()
        {
            var userId = User.FindFirstValue("UserId");
            var user = _userRepo.GetById(int.Parse(userId));
            return View(user);
        }

        // PROFİLİ GÜNCELLE
        [HttpPost]
        public async Task<IActionResult> Update(User p, IFormFile? profileImage)
        {
            var user = _userRepo.GetById(p.Id);
            if (user != null)
            {
                // 1. Yeni Bilgileri Kaydet
                user.Password = p.Password; // Şifre
                user.Email = p.Email;       // Email
                user.PhoneNumber = p.PhoneNumber; // Telefon

                // 2. Profil Resmi Yüklendi mi?
                if (profileImage != null)
                {
                    // Dosya uzantısını al (.jpg, .png)
                    string extension = Path.GetExtension(profileImage.FileName);
                    // Benzersiz bir isim oluştur (örn: user_5_guid.jpg)
                    string newImageName = "user_" + user.Id + "_" + Guid.NewGuid() + extension;

                    // Kaydedilecek yer: wwwroot/img/profiles/
                    string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "profiles");

                    // Klasör yoksa oluştur
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    string fullPath = Path.Combine(folderPath, newImageName);

                    // Dosyayı kaydet
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await profileImage.CopyToAsync(stream);
                    }

                    // Veritabanına yolunu yaz
                    user.ProfileImageUrl = "/img/profiles/" + newImageName;
                }

                _userRepo.Update(user);
                TempData["Success"] = "Profil başarıyla güncellendi!";
            }
            return RedirectToAction("Index");
        }
    }
}