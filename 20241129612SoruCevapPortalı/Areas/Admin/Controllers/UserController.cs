using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using _20241129612SoruCevapPortalı.Models;
using _20241129612SoruCevapPortalı.Repositories.Abstract;

namespace _20241129612SoruCevapPortalı.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,MainAdmin")]
    public class UserController : Controller
    {
        private readonly IRepository<User> _userRepo;

        public UserController(IRepository<User> userRepo)
        {
            _userRepo = userRepo;
        }

        [HttpGet]
        public IActionResult Index(string search, string role)
        {
            var users = _userRepo.GetAll(); 

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                users = users.Where(x =>
                    x.Username.ToLower().Contains(search) ||
                    x.FirstName.ToLower().Contains(search) ||
                    x.LastName.ToLower().Contains(search) ||
                    x.Email.ToLower().Contains(search)
                ).ToList();
            }

            if (!string.IsNullOrEmpty(role))
            {
                users = users.Where(x => x.Role == role).ToList();
            }

            return View(users);
        }

        public IActionResult Delete(int id)
        {
            var user = _userRepo.GetById(id);
            if (user != null)
            {
                if (user.Role == "MainAdmin")
                {
                    TempData["Error"] = "Ana Yönetici silinemez!";
                    return RedirectToAction("Index");
                }

                if (user.Username == User.Identity.Name)
                {
                    TempData["Error"] = "Kendini silemezsin!";
                    return RedirectToAction("Index");
                }

                
                if (user.Role == "Admin" && !User.IsInRole("MainAdmin"))
                {
                    TempData["Error"] = "Yöneticileri silme yetkiniz yok!";
                    return RedirectToAction("Index");
                }

                _userRepo.Delete(user);
            }
            return RedirectToAction("Index");
        }

        public IActionResult ChangeRole(int id)
        {
            var user = _userRepo.GetById(id);
            if (user != null)
            {
                if (user.Role == "MainAdmin")
                {
                    TempData["Error"] = "Ana Yöneticinin rolü değiştirilemez!";
                    return RedirectToAction("Index");
                }

                if (user.Username == User.Identity.Name)
                {
                    TempData["Error"] = "Kendi yetkini değiştiremezsin!";
                    return RedirectToAction("Index");
                }

                if (user.Role == "Admin")
                {
                    user.Role = "Uye"; 
                }
                else
                {
                    user.Role = "Admin"; 
                }

                _userRepo.Update(user);
            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _userRepo.GetById(id);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            if (user.Role == "MainAdmin" && !User.IsInRole("MainAdmin"))
            {
                TempData["Error"] = "Ana Yöneticinin bilgilerini düzenleyemezsiniz!";
                return RedirectToAction("Index");
            }

            return View(user);
        }

        [HttpPost]
        public IActionResult Edit(User p)
        {
            var user = _userRepo.GetById(p.Id);
            if (user != null)
            {
                user.FirstName = p.FirstName;
                user.LastName = p.LastName;
                user.Username = p.Username;
                user.Email = p.Email;
                user.PhoneNumber = p.PhoneNumber;

                if (!string.IsNullOrEmpty(p.Password))
                {
                    // DÜZELTME: Şifreyi kaydetmeden önce hash'liyoruz.
                    user.Password = _20241129612SoruCevapPortalı.Helpers.SecurityHelper.HashPassword(p.Password);
                }

                _userRepo.Update(user);
                TempData["Success"] = "Kullanıcı bilgileri başarıyla güncellendi.";
            }

            return RedirectToAction("Index");
        }
    }
}