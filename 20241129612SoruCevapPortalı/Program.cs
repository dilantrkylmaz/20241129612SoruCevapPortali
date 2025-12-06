using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using _20241129612SoruCevapPortalı.Models;
using _20241129612SoruCevapPortalı.Repositories.Abstract;
using _20241129612SoruCevapPortalı.Repositories.Concrete;

namespace _20241129612SoruCevapPortalı
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Veritabanı Bağlantısı
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 2. Repository Bağlantıları (Dependency Injection)
            // Bu satır IRepository çağrıldığında GenericRepository kullanmasını sağlar.
            builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

            // 3. Kimlik Doğrulama (Cookie Authentication)
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Login/AccessDenied";
                });

            // 4. Controller ve View Servisleri
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // 5. HTTP İstek Hattı (Pipeline)
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // BU SIRALAMA ÇOK ÖNEMLİ:
            app.UseAuthentication(); // Önce kimlik doğrula (Kimsin?)
            app.UseAuthorization();  // Sonra yetkilendir (Girebilir misin?)

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
            );

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // --- OTOMATİK ADMIN EKLEME KODU (BAŞLANGIÇ) ---
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    // Repository servisini çağır
                    var userRepo = services.GetRequiredService<IRepository<User>>();

                    // Veritabanında Admin rolüne sahip kullanıcı var mı?
                    var adminUser = userRepo.GetAll(x => x.Role == "Admin").FirstOrDefault();

                    // Yoksa ekle
                    if (adminUser == null)
                    {
                        userRepo.Add(new User
                        {
                            Username = "admin",
                            Password = "123", // Test için basit şifre
                            Role = "Admin"    // Kritik nokta burası!
                        });
                    }
                }
                catch (Exception ex)
                {
                    // Hata olursa konsola yaz (Loglama)
                    Console.WriteLine("Admin oluşturulurken hata: " + ex.Message);
                }
            }
            // --- OTOMATİK ADMIN EKLEME KODU (BİTİŞ) ---

            app.Run();
        }
    }
}