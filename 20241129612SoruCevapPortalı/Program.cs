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

          
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Login/AccessDenied";
                });

            
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

      
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var userRepo = services.GetRequiredService<IRepository<User>>();

                    var mainAdmin = userRepo.GetAll(x => x.Username == "admin").FirstOrDefault();

                    if (mainAdmin == null)
                    {
                        userRepo.Add(new User
                        {
                            Username = "admin",
                            Password = _20241129612SoruCevapPortalı.Helpers.SecurityHelper.HashPassword("123"),
                            Role = "MainAdmin", 

                            FirstName = "Sistem",
                            LastName = "Yöneticisi",
                            Email = "admin@sorucevapportali.com",
                            PhoneNumber = "5555555555"
                        });
                    }
                    else if (mainAdmin.Role != "MainAdmin")
                    {
                        mainAdmin.Role = "MainAdmin";
                        userRepo.Update(mainAdmin);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Main Admin eklenirken hata: " + ex.Message);
                }
            }
           

            app.Run();
        }
    }
}