using JWTDemo.Data;
using JWTDemo.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
namespace JWTDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create the WebApplication builder to configure services and middleware
            var builder = WebApplication.CreateBuilder(args);

            // Register MVC Controllers and configure JSON serialization options
            // Here, disabling camel case so property names remain as declared in C# classes
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                });

            // Register Swagger services to generate API documentation
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Register EF Core DbContext with SQL Server using connection string from configuration
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("EFCoreDBConnection")));

            // Register in-memory caching services for application-wide cache storage
            builder.Services.AddMemoryCache();

            // Register the ClientCacheService as singleton to maintain client info cache across requests
            builder.Services.AddSingleton<IClientCacheService, ClientCacheService>();

            // Register application services with Scoped lifetime (per HTTP request)
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IUserService, UserService>();

            // Declare a Lazy<IClientCacheService> variable to be initialized later 
            // This allows deferred resolution of the client cache service after the app is built
            Lazy<IClientCacheService>? clientCacheInstance = null;

            // Configure JWT Bearer Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // Setup token validation parameters
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true, // Validate that token issuer matches expected issuer
                        ValidIssuer = builder.Configuration["JwtSettings:Issuer"], // Expected issuer value
                        ValidateAudience = false, // Audience validated manually later
                        ValidateIssuerSigningKey = true, // Validate the token's signing key
                        ValidateLifetime = true, // Validate token expiration and not-before times

                        // Dynamically obtains the signing key based on the client_id claim,
                        // fetching the corresponding client’s secret key from cache.
                        IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
                        {
                            // Parse the incoming JWT token to extract claims
                            var jwtToken = new JwtSecurityToken(token);

                            // Extract client_id claim to identify which client signed this token
                            var clientId = jwtToken.Claims.FirstOrDefault(c => c.Type == "client_id")?.Value;

                            // If clientId or client cache is not available, return empty keys => fail validation
                            if (string.IsNullOrEmpty(clientId) || clientCacheInstance == null)
                                return Enumerable.Empty<SecurityKey>();

                            // Retrieve the client info synchronously from cache
                            var client = clientCacheInstance.Value.GetClientByClientIdAsync(clientId).Result;
                            if (client == null)
                                return Enumerable.Empty<SecurityKey>();

                            // Convert the client's stored Base64 secret into a byte array for key
                            var keyBytes = Convert.FromBase64String(client.ClientSecret);

                            // Create the symmetric security key from byte array for signature validation
                            return new[] { new SymmetricSecurityKey(keyBytes) };
                        }
                    };

                    // Additional asynchronous validation after the token is validated,
                    // confirming the client exists and audience matches the stored client URL.
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            // Extract client_id claim from the validated token
                            var clientId = context.Principal?.FindFirst("client_id")?.Value;
                            if (string.IsNullOrEmpty(clientId))
                            {
                                // Fail if claim is missing
                                context.Fail("ClientId claim missing.");
                                return;
                            }

                            if (clientCacheInstance == null)
                            {
                                context.Fail("Client Cache Instance is null");
                                return;
                            }

                            // Asynchronously get client info from cache or database
                            var client = await clientCacheInstance.Value.GetClientByClientIdAsync(clientId);
                            if (client == null)
                            {
                                // Fail if client not found
                                context.Fail("Invalid client.");
                                return;
                            }

                            // Extract audience claim from token and compare to client URL stored in DB/cache
                            var audClaim = context.Principal?.FindFirst(JwtRegisteredClaimNames.Aud)?.Value;
                            if (audClaim != client.ClientURL)
                            {
                                // Fail if audience doesn't match
                                context.Fail("Invalid audience.");
                                return;
                            }
                        }
                    };
                });

            // Build the application pipeline; after this, the services collection is read-only
            var app = builder.Build();

            // Initialize the lazy client cache instance now that the DI container is built and available
            clientCacheInstance = new Lazy<IClientCacheService>(() =>
                app.Services.GetRequiredService<IClientCacheService>());

            // Enable middleware to generate Swagger UI documentation during development
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Enforce HTTPS redirection middleware for security
            app.UseHttpsRedirection();

            // Enable authentication and authorization middleware for API endpoints
            app.UseAuthentication();
            app.UseAuthorization();

            // Map incoming HTTP requests to controller action methods
            app.MapControllers();

            // Run the application, blocking the thread to listen for requests
            app.Run();
        }
    }
}