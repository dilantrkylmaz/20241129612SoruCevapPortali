using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using _20241129612SoruCevapPortalı.Models;
using _20241129612SoruCevapPortalı.Repositories.Abstract;

namespace _20241129612SoruCevapPortalı.Controllers
{
    public class AccountController : Controller
    {
        private readonly IRepository<User> _userRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountController(IRepository<User> userRepository, IWebHostEnvironment webHostEnvironment)
        {
            _userRepository = userRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        // --- GİRİŞ YAP (LOGIN) ---
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(User p)
        {
            // Kullanıcı adı ve şifre kontrolü
            var user = _userRepository.Get(x => x.Username == p.Username && x.Password == p.Password);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role ?? "Uye"),
                    new Claim("UserId", user.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties();

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                // Admin ise Admin panele, Üye ise anasayfaya gitsin (İsteğe bağlı)
                if (user.Role == "MainAdmin" || user.Role == "Admin")
                {
                    return RedirectToAction("Dashboard", "Admin");
                }

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
            return View();
        }

        // --- KAYIT OL (REGISTER) ---
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User p, IFormFile? profileImage)
        {
            // Validasyon kontrolü (Modelde Required alanlar boş mu?)
            if (!ModelState.IsValid)
            {
                return View(p);
            }

            // 1. KULLANICI ADI KONTROLÜ
            var checkUsername = _userRepository.Get(x => x.Username == p.Username);
            if (checkUsername != null)
            {
                ViewBag.Error = "Bu kullanıcı adı zaten kullanılıyor! Lütfen başka bir ad seçiniz.";
                return View(p);
            }

            // 2. E-POSTA KONTROLÜ
            var checkEmail = _userRepository.Get(x => x.Email == p.Email);
            if (checkEmail != null)
            {
                ViewBag.Error = "Bu e-posta adresi ile daha önce kayıt olunmuş.";
                return View(p);
            }

            // 3. Profil Resmi İşlemleri
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
                p.ProfileImageUrl = "/img/profiles/" + newImageName;
            }

            // 4. Varsayılan Rol ve Kayıt
            p.Role = "Uye";
            _userRepository.Add(p);

            return RedirectToAction("Login");
        }

        // --- ÇIKIŞ YAP (LOGOUT) ---
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}