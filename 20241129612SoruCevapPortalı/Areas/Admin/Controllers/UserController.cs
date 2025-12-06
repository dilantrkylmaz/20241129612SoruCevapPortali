using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using _20241129612SoruCevapPortalı.Models;
using _20241129612SoruCevapPortalı.Repositories.Abstract;

namespace _20241129612SoruCevapPortalı.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,MainAdmin")] // Sadece Admin girebilir
    public class UserController : Controller
    {
        private readonly IRepository<User> _userRepo;

        public UserController(IRepository<User> userRepo)
        {
            _userRepo = userRepo;
        }

        // 1. ÜYE LİSTESİ
        [HttpGet]
        public IActionResult Index(string search)
        {
            // Arama kutusu boşsa hepsini getir
            if (string.IsNullOrEmpty(search))
            {
                return View(_userRepo.GetAll());
            }

            // Doluysa filtrele (Büyük küçük harf duyarsız yapmaya çalışıyoruz)
            search = search.ToLower();
            var filteredUsers = _userRepo.GetAll(x =>
                x.Username.ToLower().Contains(search) ||
                x.FirstName.ToLower().Contains(search) ||
                x.LastName.ToLower().Contains(search) ||
                x.Email.ToLower().Contains(search)
            );

            return View(filteredUsers);
        }

        // 2. ÜYE SİLME
        public IActionResult Delete(int id)
        {
            var user = _userRepo.GetById(id);
            if (user != null)
            {
                // 1. KİMSE MAIN ADMIN'İ SİLEMEZ
                if (user.Role == "MainAdmin")
                {
                    TempData["Error"] = "Ana Yönetici silinemez!";
                    return RedirectToAction("Index");
                }

                // 2. KİMSE KENDİNİ SİLEMEZ
                if (user.Username == User.Identity.Name)
                {
                    TempData["Error"] = "Kendini silemezsin!";
                    return RedirectToAction("Index");
                }

                // 3. ALT YÖNETİCİLER (Admin), DİĞER YÖNETİCİLERİ SİLEMEZ
                // Sadece MainAdmin herkesi silebilir.
                if (user.Role == "Admin" && !User.IsInRole("MainAdmin"))
                {
                    TempData["Error"] = "Yöneticileri silme yetkiniz yok!";
                    return RedirectToAction("Index");
                }

                _userRepo.Delete(user);
            }
            return RedirectToAction("Index");
        }

        // 3. ROL DEĞİŞTİRME
        public IActionResult ChangeRole(int id)
        {
            var user = _userRepo.GetById(id);
            if (user != null)
            {
                // MAIN ADMIN ROLÜ DEĞİŞMEZ
                if (user.Role == "MainAdmin")
                {
                    TempData["Error"] = "Ana Yöneticinin rolü değiştirilemez!";
                    return RedirectToAction("Index");
                }

                // KENDİ ROLÜNÜ DEĞİŞTİRME
                if (user.Username == User.Identity.Name)
                {
                    TempData["Error"] = "Kendi yetkini değiştiremezsin!";
                    return RedirectToAction("Index");
                }

                // --- ROL DÖNGÜSÜ (Toggle) ---
                if (user.Role == "Admin")
                {
                    user.Role = "Uye"; // Admin -> Üye oldu
                }
                else
                {
                    user.Role = "Admin"; // Üye -> Admin oldu
                }

                _userRepo.Update(user);
            }
            return RedirectToAction("Index");
        }
        // 4. ÜYE DÜZENLEME SAYFASI (GET)
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _userRepo.GetById(id);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            // MainAdmin dışındakiler MainAdmin'i düzenleyemesin
            if (user.Role == "MainAdmin" && !User.IsInRole("MainAdmin"))
            {
                TempData["Error"] = "Ana Yöneticinin bilgilerini düzenleyemezsiniz!";
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // 5. ÜYE DÜZENLEME İŞLEMİ (POST)
        [HttpPost]
        public IActionResult Edit(User p)
        {
            var user = _userRepo.GetById(p.Id);
            if (user != null)
            {
                // Temel Bilgileri Güncelle
                user.FirstName = p.FirstName;
                user.LastName = p.LastName;
                user.Username = p.Username;
                user.Email = p.Email;
                user.PhoneNumber = p.PhoneNumber;

                // Şifre alanı boş değilse şifreyi de güncelle (Boşsa eskisi kalsın)
                if (!string.IsNullOrEmpty(p.Password))
                {
                    user.Password = p.Password;
                }

                // Rolü de buradan güncelleyebilsin (İsteğe bağlı)
                // user.Role = p.Role; 

                _userRepo.Update(user);
                TempData["Success"] = "Kullanıcı bilgileri başarıyla güncellendi.";
            }

            return RedirectToAction("Index");
        }
    }
}