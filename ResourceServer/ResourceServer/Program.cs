using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ResourceServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Register controllers and disable the default camel-casing of JSON property names.
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

            // Register JWT Authentication services.
            builder.Services.AddAuthentication(options =>
            {
                // Set the default authentication scheme to JWT Bearer.
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Configure how the JWT token is validated.
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,               // Set to false if you don't want to Validate Issuer.
                    ValidateAudience = false,            // Set to true if you want to validate the audience.
                    ValidateLifetime = true,             // Ensures the token hasn't expired.
                    ValidateIssuerSigningKey = true,     // Validates that the token is signed with a trusted key.
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty))
                };
            });

            // Register Swagger/OpenAPI services for API documentation and testing.
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Enable Swagger UI in development mode.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Add the authentication and authorization middleware.
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}