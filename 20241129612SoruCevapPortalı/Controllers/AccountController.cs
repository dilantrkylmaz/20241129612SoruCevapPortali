using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using _20241129612SoruCevapPortalı.Models;

namespace _20241129612SoruCevapPortalı.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(User p)
        {
            // Identity şifre kontrolünü kendi içinde (PasswordHash üzerinden) yapar.
            // Password alanı NotMapped olduğu için veri tabanı sorgusuna dahil edilemez.
            var result = await _signInManager.PasswordSignInAsync(p.UserName, p.Password, false, false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(p.UserName);

                // Rol kontrolü yaparak yönlendirme
                if (await _userManager.IsInRoleAsync(user, "MainAdmin") || await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    // "Admin" alanındaki (Area), "Dashboard" isimli Controller'ın "Index" metoduna git.
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
            return View();
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(User model, IFormFile? profileImage)
        {
            // Identity'de UserName ve Email zorunludur.
            model.UserName = model.Email;

            if (profileImage != null)
            {
                string extension = Path.GetExtension(profileImage.FileName);
                string newImageName = "user_" + Guid.NewGuid() + extension;
                string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "profiles");

                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                using (var stream = new FileStream(Path.Combine(folderPath, newImageName), FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }
                model.ProfileImageUrl = "/img/profiles/" + newImageName;
            }

            // Şifreyi Identity kendi yöntemiyle hashleyerek kaydeder.
            var result = await _userManager.CreateAsync(model, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(model, "Uye");
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}