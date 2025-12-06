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

        public AccountController(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
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
            // Kullanıcıyı veritabanında ara
            var user = _userRepository.GetAll(x => x.Username == p.Username && x.Password == p.Password).FirstOrDefault();

            if (user != null)
            {
                // Kullanıcı bulunduysa, kimlik bilgilerini (Claims) hazırla
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role ?? "Uye"), // Rolü yoksa 'Uye' olsun
                    new Claim("UserId", user.Id.ToString()) // ID'yi de tutalım, lazım olacak
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties();

                // Çerezi (Cookie) oluştur ve kullanıcıyı içeri al
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                return RedirectToAction("Index", "Home");
            }

            // Hatalı giriş
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
        public IActionResult Register(User p)
        {
            // Aynı isimde kullanıcı var mı?
            var existingUser = _userRepository.GetAll(x => x.Username == p.Username).FirstOrDefault();
            if (existingUser != null)
            {
                ViewBag.Error = "Bu kullanıcı adı zaten alınmış.";
                return View();
            }

            // Yeni kullanıcıyı ekle
            p.Role = "Uye"; // Varsayılan rol
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