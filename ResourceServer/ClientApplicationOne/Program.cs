using ClientApplicationOne.Models;
namespace ClientApplicationOne
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the dependency injection container.
            // Add MVC services with support for views and controllers.
            builder.Services.AddControllersWithViews();

            // Add distributed memory cache and configure session management.
            builder.Services.AddDistributedMemoryCache(); // Required for session storage
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout period.
                options.Cookie.HttpOnly = true; // Prevents access to the cookie via client-side scripts.
                options.Cookie.IsEssential = true; // Marks the cookie as essential for GDPR compliance.
            });

            // Register the HttpClient factory and the UserService.
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<UserAuthenticationService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline (middleware).
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // Enable session state for the application, allowing the app to store user data between requests.
            app.UseSession();

            // Enable authentication middleware to check if the user is authenticated.
            app.UseAuthentication();

            // Enable authorization middleware to enforce user role and permission-based access control.
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}