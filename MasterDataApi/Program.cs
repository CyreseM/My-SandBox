// =============================================================
// Program.cs
//
// This is the ENTRY POINT of the application — the first code
// that runs when you start the server.
//
// We do two things here:
//   1. Register services into the Dependency Injection (DI) container
//   2. Configure the HTTP request pipeline (middleware)
//
// Think of DI like a "parts warehouse": you register what's available,
// and ASP.NET Core automatically provides them wherever they're needed.
// =============================================================

using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MasterDataApi.Data;
using MasterDataApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════
// SECTION 1 — Register Services (the DI container)
// ═══════════════════════════════════════════════════════════════

// ── Database ─────────────────────────────────────────────────
// SQLite keeps local data in a file and supports EF Core migrations.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")
        ?? "Data Source=masterdata.db"));

// ── Controllers ───────────────────────────────────────────────
// Scans for all classes ending in "Controller" and registers them
builder.Services.AddControllers();

// ── Application Services ──────────────────────────────────────
// AddScoped = a new instance per HTTP request.
// When a controller asks for ICompanyService, DI gives it CompanyService.
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserAssignmentService, UserAssignmentService>();
builder.Services.AddScoped<IBulkUploadService, BulkUploadService>();

// ── JWT Authentication ────────────────────────────────────────
// This configures the app to read and validate Bearer tokens.
// In production, replace the key and issuer with real secrets from config.
var jwtKey = builder.Configuration["Jwt:Key"] ?? "SuperSecretDevKeyThatIsAtLeast32CharsLong!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MasterDataApi";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = false,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtIssuer,
            // The signing key — must match the key used to generate tokens
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        // For development: don't fail on expired tokens
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine($"[Auth] Token validation failed: {ctx.Exception.Message}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ── Swagger / OpenAPI ─────────────────────────────────────────
// Swagger generates an interactive API explorer at /swagger
// All the /// XML comments on controllers/DTOs appear here automatically
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // ── API metadata shown at the top of Swagger UI ──────────
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Master Data & User Assignment API",
        Version     = "v1.0.0",
        Description = """
            The **Master Data & User Assignment API** is the organizational backbone of the platform.

            ## What this API does
            - **Master Data Management** — CRUD for Companies, Departments, and Roles
            - **User Assignment** — Link users to their organizational position
            - **Bulk Operations** — Upload Excel files for mass onboarding

            ## Organizational hierarchy
            ```
            User → Company → Department → Role
            ```

            ## Quick Start
            1. 🔑 Click **Authorize** and paste your Bearer token
            2. 🏢 Create a Company via `POST /api/companies`
            3. 🏗️ Create Departments via `POST /api/departments`
            4. 🎭 Create Roles via `POST /api/roles`
            5. 👤 Assign a User via `POST /api/users/assign`

            ## Authentication
            All endpoints require a JWT Bearer token in the `Authorization` header.
            For local testing, use the **dev token** shown below.

            ## 🧪 Dev Token (for testing only)
            ```
            eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJNYXN0ZXJEYXRhQXBpIiwibmFtZSI6IkRldlVzZXIiLCJpYXQiOjE3MDAwMDAwMDB9.placeholder
            ```
            > ⚠️ The dev token is auto-generated on startup. Use the `/api/auth/dev-token` endpoint to get one.
            """,
        Contact = new OpenApiContact
        {
            Name  = "API Support",
            Email = "support@yourapp.com"
        }
    });

    // ── JWT Authorization in Swagger UI ──────────────────────
    // This adds the 🔒 "Authorize" button to Swagger UI
    // so you can paste a token and test protected endpoints
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = """
            Paste your JWT token here (without the 'Bearer' prefix — Swagger adds it automatically).

            **How to get a token for local testing:**
            Call `GET /api/auth/dev-token` and paste the returned token here.
            """,
    });

    // Apply the security scheme globally — all endpoints require auth
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // ── Read XML comments from the compiled docs file ─────────
    // This makes your /// comments show up in Swagger UI
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);

    // Enable [SwaggerOperation], [SwaggerResponse] annotations
    options.EnableAnnotations();

    // Order endpoints alphabetically by tag name
    options.OrderActionsBy(api => $"{api.GroupName}_{api.RelativePath}_{api.HttpMethod}");
});

// ── CORS ──────────────────────────────────────────────────────
// CORS = Cross-Origin Resource Sharing.
// This lets a front-end app on a different domain call your API.
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevPolicy", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// ═══════════════════════════════════════════════════════════════
// SECTION 2 — Seed the in-memory database with initial data
// ═══════════════════════════════════════════════════════════════

// Apply pending migrations and seed data in OnModelCreating.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// ═══════════════════════════════════════════════════════════════
// SECTION 3 — Configure the HTTP pipeline (middleware chain)
// ORDER MATTERS here — each middleware wraps the next one
// ═══════════════════════════════════════════════════════════════

// Always serve Swagger in all environments for this project
// In production, you'd guard this with: if (app.Environment.IsDevelopment())
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Master Data API v1");
    options.RoutePrefix        = string.Empty; // Serve Swagger at root URL "/"
    options.DocumentTitle      = "Master Data API";
    options.DefaultModelsExpandDepth(2);  // Auto-expand model schemas
    options.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
    options.DisplayRequestDuration();     // Show how long each call took
    options.EnableDeepLinking();          // Shareable URLs for specific endpoints
    options.EnableFilter();               // Add a search box to filter endpoints
});

app.UseHttpsRedirection();
app.UseCors("DevPolicy");

// Authentication MUST come before Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controller routes (reads [Route] attributes from controller classes)
app.MapControllers();

// ── Dev Token endpoint ────────────────────────────────────────
// A quick way to get a test token without setting up a full auth server.
// REMOVE THIS IN PRODUCTION.
app.MapGet("/api/auth/dev-token", () =>
{
    // In a real app you'd validate credentials and issue a real token.
    // Here we just return a static dev token for local Swagger testing.
    var token = GenerateDevToken(jwtKey, jwtIssuer);
    return Results.Ok(new { token, note = "Use this in Swagger's Authorize dialog (paste just the token, no 'Bearer' prefix)" });
})
.AllowAnonymous()
.WithTags("Auth")
.WithSummary("Get a dev token for Swagger testing")
.WithDescription("⚠️ Development only — remove this endpoint in production. Returns a JWT token you can use in the Authorize dialog to test protected endpoints.")
;

app.Run();

// ── Helper: generate a JWT for local testing ─────────────────
static string GenerateDevToken(string key, string issuer)
{
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new System.Security.Claims.Claim("iss", issuer),
        new System.Security.Claims.Claim("name", "DevUser"),
        new System.Security.Claims.Claim("role", "Admin"),
    };

    var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
        issuer:    issuer,
        claims:    claims,
        expires:   DateTime.UtcNow.AddDays(7),     // Expires in 7 days
        signingCredentials: credentials
    );

    return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
}
