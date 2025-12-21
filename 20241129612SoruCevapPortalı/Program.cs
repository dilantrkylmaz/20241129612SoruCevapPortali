using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using _20241129612SoruCevapPortalı.Models;
using _20241129612SoruCevapPortalı.Repositories.Abstract;
using _20241129612SoruCevapPortalı.Repositories.Concrete;
using _20241129612SoruCevapPortalı.Hubs;

namespace _20241129612SoruCevapPortalı
{
    public class Program
    {
        // Metot imzası asenkron yapıya uygun hale getirildi
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Veritabanı bağlantısı
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Identity yapılandırması
            builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 3;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

            // Identity Cookie ayarları
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.LogoutPath = "/Account/Logout";
                options.Cookie.Name = "SoruCevapPortal.Auth";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
            });

            builder.Services.AddSignalR();
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
            );

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapHub<PortalHub>("/portalHub");

            // Başlangıç Verileri (Seed Data) - Tamamen asenkron yapıldı
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var userManager = services.GetRequiredService<UserManager<User>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

                    // 1. Rolleri oluştur
                    string[] roleNames = { "MainAdmin", "Admin", "Uye" };
                    foreach (var roleName in roleNames)
                    {
                        if (!await roleManager.RoleExistsAsync(roleName))
                        {
                            await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                        }
                    }

                    // 2. Admin kullanıcısını kontrol et
                    var adminUser = await userManager.FindByNameAsync("admin");
                    if (adminUser == null)
                    {
                        var user = new User
                        {
                            UserName = "admin",
                            Email = "admin@sorucevapportali.com",
                            FirstName = "Sistem",
                            LastName = "Yöneticisi",
                            PhoneNumber = "5555555555"
                        };

                        var result = await userManager.CreateAsync(user, "123");
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(user, "MainAdmin");
                        }
                    }
                    else
                    {
                        // Rol kontrolü asenkron yapıldı
                        if (!await userManager.IsInRoleAsync(adminUser, "MainAdmin"))
                        {
                            await userManager.AddToRoleAsync(adminUser, "MainAdmin");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Seed Data Hatası: " + ex.Message);
                }
            }

            await app.RunAsync();
        }
    }
}