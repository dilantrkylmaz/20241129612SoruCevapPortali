using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using _20241129612SoruCevapPortalı.Models;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace _20241129612SoruCevapPortalı.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IWebHostEnvironment webHostEnvironment, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(User p)
        {
            // Kullanıcı adını veya e-postayı kontrol ederek kullanıcıyı buluyoruz
            var user = await _userManager.FindByNameAsync(p.UserName) ?? await _userManager.FindByEmailAsync(p.UserName);

            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user.UserName, p.Password, false, false);
                if (result.Succeeded)
                {
                    if (await _userManager.IsInRoleAsync(user, "MainAdmin") || await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                    }
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Error = "Giriş bilgileri hatalı!";
            return View();
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(User model, IFormFile? profileImage)
        {
            // DÜZELTME: model.UserName = model.Email satırı kaldırıldı. 
            // Kullanıcı adı artık View'dan (formdan) gelen değer olacak.

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

        // --- ŞİFRE SIFIRLAMA METOTLARI ---

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email)) return View();

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ViewBag.Message = "İşlem başarıyla tamamlandı (Güvenlik gereği sadece kayıtlı e-postalara gönderim yapılır).";
                return View();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, token = token }, protocol: HttpContext.Request.Scheme);

            await _emailSender.SendEmailAsync(email, "Şifre Sıfırlama", $"Şifrenizi sıfırlamak için <a href='{callbackUrl}'>buraya tıklayın</a>.");

            ViewBag.Message = "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi.";
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string userId, string token)
        {
            if (userId == null || token == null) return RedirectToAction("Login");
            return View(new ResetPasswordViewModel { Token = token, UserId = userId });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return RedirectToAction("Login");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                TempData["Success"] = "Şifreniz güncellendi. Giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}