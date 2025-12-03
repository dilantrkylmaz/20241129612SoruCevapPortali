using Microsoft.EntityFrameworkCore;
using _20241129612SoruCevapPortalı.Models;

namespace _20241129612SoruCevapPortalı
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Veritabanı Bağlantısı (SQL Server)
            // appsettings.json dosyasındaki "DefaultConnection" ismini kullanır.
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 2. Servislerin Eklenmesi
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // 3. HTTP İstek Hattının Yapılandırılması
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}