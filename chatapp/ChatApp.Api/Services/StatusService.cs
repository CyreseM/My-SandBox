using ChatApp.Api.DTOs;
using ChatApp.Core.Models;
using ChatApp.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Api.Services;

public class StatusService
{
    private readonly AppDbContext _db;
    public StatusService(AppDbContext db) => _db = db;

    private IQueryable<Status> ActiveStatuses() =>
        _db.Statuses.Where(s => s.ExpiresAt > DateTime.UtcNow);

    public async Task<List<StatusGroupDto>> GetContactStatusesAsync(Guid userId)
    {
        // Get users from DM chats
        var directPartnerIds = await _db.ChatMembers
            .Where(m => m.UserId == userId)
            .Join(_db.Chats.Where(c => c.Type == ChatType.Direct),
                  m => m.ChatId, c => c.Id, (m, c) => c.Id)
            .Join(_db.ChatMembers.Where(m => m.UserId != userId),
                  cid => cid, m => m.ChatId, (cid, m) => m.UserId)
            .Distinct()
            .ToListAsync();

        var statuses = await ActiveStatuses()
            .Where(s => directPartnerIds.Contains(s.UserId))
            .Include(s => s.User)
            .Include(s => s.Views)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        var viewedIds = await _db.StatusViews
            .Where(sv => sv.ViewerId == userId)
            .Select(sv => sv.StatusId)
            .ToListAsync();

        return statuses
            .GroupBy(s => s.UserId)
            .Select(g => new StatusGroupDto(
                g.Key,
                g.First().User.DisplayName,
                g.First().User.AvatarUrl,
                g.Select(s => s.Id).ToList(),
                g.Any(s => !viewedIds.Contains(s.Id))))
            .ToList();
    }

    public async Task<Status> CreateAsync(CreateStatusDto dto, Guid userId)
    {
        if (!Enum.TryParse<StatusMediaType>(dto.MediaType, out var mediaType))
            mediaType = StatusMediaType.None;

        var status = new Status
        {
            Id = Guid.NewGuid(), UserId = userId,
            Content = dto.Content, MediaUrl = dto.MediaUrl,
            MediaType = mediaType, BackgroundColor = dto.BackgroundColor,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
        _db.Statuses.Add(status);
        await _db.SaveChangesAsync();

        return await _db.Statuses.Include(s => s.User).Include(s => s.Views)
            .FirstAsync(s => s.Id == status.Id);
    }

    public async Task<Status?> GetByIdAsync(Guid statusId) =>
        await _db.Statuses.Include(s => s.User).Include(s => s.Views)
            .FirstOrDefaultAsync(s => s.Id == statusId && s.ExpiresAt > DateTime.UtcNow);

    public async Task DeleteAsync(Guid statusId, Guid userId)
    {
        var s = await _db.Statuses.FindAsync(statusId);
        if (s != null && s.UserId == userId) { _db.Statuses.Remove(s); await _db.SaveChangesAsync(); }
    }

    public async Task RecordViewAsync(Guid statusId, Guid viewerId)
    {
        if (!await _db.StatusViews.AnyAsync(sv => sv.StatusId == statusId && sv.ViewerId == viewerId))
        {
            _db.StatusViews.Add(new StatusView
            {
                StatusId = statusId, ViewerId = viewerId, ViewedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }
    }

    public async Task<List<UserDto>> GetViewersAsync(Guid statusId, Guid ownerId)
    {
        var status = await _db.Statuses.FindAsync(statusId);
        if (status == null || status.UserId != ownerId) return new();

        return await _db.StatusViews
            .Where(sv => sv.StatusId == statusId)
            .Include(sv => sv.Viewer)
            .Select(sv => UserDto.From(sv.Viewer))
            .ToListAsync();
    }
}
