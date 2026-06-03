using HMACServerApp.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                // This will use the property names as defined in the C# model
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Configure the ConnectionString and DbContext class
builder.Services.AddDbContext<HMACDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("EFCoreDBConnection"));
});
//Adding In-Memory Caching
builder.Services.AddMemoryCache();
// Register the ClientSecretService
builder.Services.AddScoped<ClientSecretService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Register HMAC middleware so it runs before authorization and controllers
app.UseMiddleware<HMACAuthenticationMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
