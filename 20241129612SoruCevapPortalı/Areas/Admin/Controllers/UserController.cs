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
        public IActionResult Index()
        {
            // Tüm kullanıcıları getir (Repository'de Include varsa soruları da çekebiliriz)
            // Şimdilik sadece User listesi yeterli
            var users = _userRepo.GetAll();
            return View(users);
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
    }
}