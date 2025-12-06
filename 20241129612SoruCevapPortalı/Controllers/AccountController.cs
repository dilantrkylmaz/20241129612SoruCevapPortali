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
        private readonly IWebHostEnvironment _webHostEnvironment; // Dosya kaydetmek için

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
            // Validasyon hatası olsa bile giriş denemesi yapabilmeli (Sadece Username/Password yeterli)
            var user = _userRepository.GetAll(x => x.Username == p.Username && x.Password == p.Password).FirstOrDefault();

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
            if (ModelState.IsValid)
            {
                // Aynı isimde kullanıcı var mı?
                var existingUser = _userRepository.GetAll(x => x.Username == p.Username || x.Email == p.Email).FirstOrDefault();
                if (existingUser != null)
                {
                    ViewBag.Error = "Bu kullanıcı adı veya e-posta zaten kayıtlı.";
                    return View(p);
                }

                // 1. Profil Resmi Yüklendi mi?
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
                // Yüklenmediyse NULL kalacak, biz View tarafında avatar göstereceğiz.

                p.Role = "Uye";
                _userRepository.Add(p);

                return RedirectToAction("Login");
            }

            return View(p);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}