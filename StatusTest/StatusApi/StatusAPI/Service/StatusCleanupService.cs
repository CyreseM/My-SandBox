using System;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StatusAPI.Data;
using StatusAPI.Hubs;

namespace StatusAPI.Service;

public class StatusCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StatusCleanupService> _logger;

    public StatusCleanupService(IServiceProvider serviceProvider,
        ILogger<StatusCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<StatusDbContext>();
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<StatusHub>>();

                var expiredStatuses = await context.Statuses
                    .Where(s => s.ExpiresAt <= DateTime.UtcNow)
                    .ToListAsync(stoppingToken);

                if (expiredStatuses.Any())
                {
                    var expiredIds = expiredStatuses.Select(s => s.Id).ToList();
                    context.Statuses.RemoveRange(expiredStatuses);
                    await context.SaveChangesAsync(stoppingToken);

                    // Notify clients about expired statuses
                    foreach (var id in expiredIds)
                    {
                        await hubContext.Clients.Group("StatusUpdates")
                            .SendAsync("StatusExpired", id, stoppingToken);
                    }

                    _logger.LogInformation($"Cleaned up {expiredStatuses.Count} expired statuses");
                }

                // Run cleanup every 5 minutes
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during status cleanup");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
