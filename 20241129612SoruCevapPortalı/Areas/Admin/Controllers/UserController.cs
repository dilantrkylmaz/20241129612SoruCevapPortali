using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using _20241129612SoruCevapPortalı.Models;

namespace _20241129612SoruCevapPortalı.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,MainAdmin")]
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public UserController(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Index(string search, string role)
        {
            var users = _userManager.Users.ToList();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                users = users.Where(x =>
                    x.UserName.ToLower().Contains(search) ||
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

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user != null)
            {
                if (user.Role == "MainAdmin")
                {
                    TempData["Error"] = "Ana Yönetici silinemez!";
                    return RedirectToAction("Index");
                }

                if (user.UserName == User.Identity.Name)
                {
                    TempData["Error"] = "Kendini silemezsin!";
                    return RedirectToAction("Index");
                }

                if (user.Role == "Admin" && !User.IsInRole("MainAdmin"))
                {
                    TempData["Error"] = "Yöneticileri silme yetkiniz yok!";
                    return RedirectToAction("Index");
                }

                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ChangeRole(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user != null)
            {
                if (user.Role == "MainAdmin")
                {
                    TempData["Error"] = "Ana Yöneticinin rolü değiştirilemez!";
                    return RedirectToAction("Index");
                }

                if (user.UserName == User.Identity.Name)
                {
                    TempData["Error"] = "Kendi yetkini değiştiremezsin!";
                    return RedirectToAction("Index");
                }

                if (user.Role == "Admin")
                {
                    user.Role = "Uye";
                    await _userManager.RemoveFromRoleAsync(user, "Admin");
                    await _userManager.AddToRoleAsync(user, "Uye");
                }
                else
                {
                    user.Role = "Admin";
                    await _userManager.RemoveFromRoleAsync(user, "Uye");
                    await _userManager.AddToRoleAsync(user, "Admin");
                }

                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
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
        public async Task<IActionResult> Edit(User p)
        {
            var user = await _userManager.FindByIdAsync(p.Id.ToString());
            if (user != null)
            {
                user.FirstName = p.FirstName;
                user.LastName = p.LastName;
                user.UserName = p.UserName;
                user.Email = p.Email;
                user.PhoneNumber = p.PhoneNumber;

                if (!string.IsNullOrEmpty(p.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, token, p.Password);
                }

                await _userManager.UpdateAsync(user);
                TempData["Success"] = "Kullanıcı bilgileri başarıyla güncellendi.";
            }

            return RedirectToAction("Index");
        }
    }
}