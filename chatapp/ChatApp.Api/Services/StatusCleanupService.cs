using ChatApp.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Api.Services;

public class StatusCleanupService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<StatusCleanupService> _logger;

    public StatusCleanupService(
        IServiceProvider services,
        IWebHostEnvironment env,
        ILogger<StatusCleanupService> logger)
    {
        _services = services;
        _env = env;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run on startup, then periodically
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredStatusesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while cleaning up expired statuses");
            }

            // Wait 1 hour between cleanup runs
            try
            {
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Ignore if cancellation requested
            }
        }
    }

    private async Task CleanupExpiredStatusesAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;

        var expired = await db.Statuses
            .Where(s => s.ExpiresAt <= now)
            .ToListAsync(cancellationToken);

        if (!expired.Any())
        {
            return;
        }

        var webRoot = _env.WebRootPath ?? "wwwroot";
        var uploadsDir = Path.Combine(webRoot, "uploads");

        foreach (var status in expired)
        {
            if (!string.IsNullOrWhiteSpace(status.MediaUrl))
            {
                try
                {
                    var mediaUrl = status.MediaUrl!;
                    var uploadsIndex = mediaUrl.IndexOf("/uploads/", StringComparison.OrdinalIgnoreCase);
                    if (uploadsIndex >= 0)
                    {
                        var relativePath = mediaUrl[(uploadsIndex + "/uploads/".Length)..];
                        var filePath = Path.Combine(uploadsDir, relativePath);

                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete media file for expired status {StatusId}", status.Id);
                }
            }
        }

        db.Statuses.RemoveRange(expired);
        await db.SaveChangesAsync(cancellationToken);
    }
}

