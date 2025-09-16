using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StatusAPI.Data;
using StatusAPI.Hubs;
using StatusAPI.Service;
using System.ComponentModel.DataAnnotations;

// Swagger/OpenAPI
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<StatusDbContext>(options =>
    options.UseInMemoryDatabase("StatusDb"));
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddHostedService<StatusCleanupService>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add Swagger/OpenAPI services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Status API", Version = "v1" });
});

var app = builder.Build();


// Enable Swagger middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Status API v1");
    c.RoutePrefix = string.Empty; // Swagger UI at root
});

app.UseCors();
app.UseRouting();
app.UseStaticFiles();
app.MapControllers();
app.MapHub<StatusHub>("/statusHub");

app.Run();
