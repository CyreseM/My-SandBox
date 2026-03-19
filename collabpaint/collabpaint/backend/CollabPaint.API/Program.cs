using System.Text;
using CollabPaint.API.Data;
using CollabPaint.API.Hubs;
using CollabPaint.API.Models;
using CollabPaint.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ── Database ─────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=collabpaint.db"));

// ── Identity ─────────────────────────────────────────────────────────────────
builder.Services
    .AddIdentityCore<AppUser>(o =>
    {
        o.Password.RequireDigit           = false;
        o.Password.RequiredLength         = 6;
        o.Password.RequireNonAlphanumeric = false;
        o.Password.RequireUppercase       = false;
        o.User.RequireUniqueEmail         = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager<SignInManager<AppUser>>()
    .AddDefaultTokenProviders();

// ── JWT ───────────────────────────────────────────────────────────────────────
var jwtKey = builder.Configuration["JwtSettings:SecretKey"]
    ?? "dev-secret-key-change-in-production-must-be-32chars!!";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey        = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer          = true,
            ValidIssuer             = builder.Configuration["JwtSettings:Issuer"]   ?? "CollabPaintAPI",
            ValidateAudience        = true,
            ValidAudience           = builder.Configuration["JwtSettings:Audience"] ?? "CollabPaintClient",
            ValidateLifetime        = true,
            ClockSkew               = TimeSpan.Zero,
        };
        // SignalR passes token via query string
        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) && ctx.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    ctx.Token = token;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ── App services ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<ITokenService,   TokenService>();
builder.Services.AddScoped<ISessionService, SessionService>();

// ── SignalR ───────────────────────────────────────────────────────────────────
builder.Services.AddSignalR(o =>
{
    o.EnableDetailedErrors      = builder.Environment.IsDevelopment();
    o.MaximumReceiveMessageSize = 512 * 1024;
});

// ── CORS ──────────────────────────────────────────────────────────────────────
var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173", "https://localhost:5173"];

builder.Services.AddCors(o => o.AddPolicy("Frontend", p =>
    p.WithOrigins(origins).AllowAnyMethod().AllowAnyHeader().AllowCredentials()));

builder.Services.AddControllers();

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<PaintHub>("/hubs/paint");
app.Run();
