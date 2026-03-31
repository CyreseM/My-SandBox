using AuthenticationServer.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AuthenticationServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Adds support for controllers to the application.
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // Disable Camel Case
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                });

            // Configures the ApplicationDbContext to use SQL Server with the connection string from the configuration file.
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("AuthenticationServerDBConnection")));

            // Adding ASP.NET Core Identity services.
            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders(); //Enable support for Password Reset, Email Confirmation, Phone Verification, and 2FA

            // Adding and configuring authentication to use JWT Bearer tokens.
            builder.Services.AddAuthentication(options =>
            {
                //Default Authentication Scheme
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            // Adding the JwtBearer authentication handler to validate incoming JWT tokens.
            .AddJwtBearer(options =>
            {
                // Configuring the parameters for JWT token validation.
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true, // Ensure the token's issuer matches the expected issuer.
                    ValidateAudience = false, // Ensure the token's audience matches the expected audience.
                    ValidateLifetime = true, // Validate that the token has not expired.
                    ValidateIssuerSigningKey = true, // Ensure the token is signed by a trusted signing key.
                    ValidIssuer = builder.Configuration["Jwt:Issuer"], // The expected issuer, retrieved from configuration (appsettings.json).
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty)) // The symmetric key used to sign the JWT, also from configuration (appsettings.json).
                };
            });

            // Adds Swagger/OpenAPI documentation.
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configuring the HTTP request pipeline (middleware components).
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Enable authentication middleware.
            app.UseAuthentication();

            // Enable authorization middleware.
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}