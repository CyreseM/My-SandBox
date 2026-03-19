using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CollabPaint.API.Data;
using CollabPaint.API.DTOs;
using CollabPaint.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CollabPaint.API.Services;

// ── Token ───────────────────────────────────────────────────────────────────

public interface ITokenService { string CreateToken(AppUser user); }

public class TokenService : ITokenService
{
    private readonly IConfiguration _cfg;
    public TokenService(IConfiguration cfg) => _cfg = cfg;

    public string CreateToken(AppUser user)
    {
        var secret   = _cfg["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT key missing");
        var issuer   = _cfg["JwtSettings:Issuer"]    ?? "CollabPaintAPI";
        var audience = _cfg["JwtSettings:Audience"]  ?? "CollabPaintClient";
        var expiry   = int.Parse(_cfg["JwtSettings:ExpiryInMinutes"] ?? "1440");

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,        user.Id),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? user.Id),
            new Claim(JwtRegisteredClaimNames.Email,      user.Email    ?? ""),
            new Claim("displayName",                      user.DisplayName),
            new Claim(JwtRegisteredClaimNames.Jti,        Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(issuer, audience, claims,
            expires: DateTime.UtcNow.AddMinutes(expiry), signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// ── Session ──────────────────────────────────────────────────────────────────

public interface ISessionService
{
    Task<SessionDto>        CreateAsync(string ownerId, string name);
    Task<List<SessionDto>>  GetAllForUserAsync(string userId);
    Task<SessionDto?>       GetByIdAsync(Guid id);
    Task                    DeleteAsync(Guid id, string requestingUserId);
    Task<List<StrokeDataDto>> GetStrokesAsync(Guid sessionId);
    Task                    SaveStrokeAsync(Guid sessionId, string userId, StrokeDataDto stroke);
    Task                    SaveSnapshotAsync(Guid sessionId, string snapshot);
    Task                    AddParticipantAsync(Guid sessionId, string userId);
    Task                    RemoveParticipantAsync(Guid sessionId, string userId);
}

public class SessionService : ISessionService
{
    private readonly AppDbContext _db;
    public SessionService(AppDbContext db) => _db = db;

    private static SessionDto Map(PaintSession s) => new(
        s.Id, s.Name, s.OwnerId,
        s.Owner?.DisplayName ?? s.Owner?.UserName ?? "",
        s.IsCollaborative, s.CreatedAt,
        s.Participants.Count,
        s.CanvasSnapshotBase64);

    public async Task<SessionDto> CreateAsync(string ownerId, string name)
    {
        var ses = new PaintSession { Name = name, OwnerId = ownerId };
        _db.PaintSessions.Add(ses);
        _db.SessionParticipants.Add(new SessionParticipant { SessionId = ses.Id, UserId = ownerId });
        await _db.SaveChangesAsync();
        return (await GetByIdAsync(ses.Id))!;
    }

    public async Task<List<SessionDto>> GetAllForUserAsync(string userId) =>
        (await _db.PaintSessions
            .Include(s => s.Owner).Include(s => s.Participants)
            .Where(s => s.OwnerId == userId || s.Participants.Any(p => p.UserId == userId))
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync())
        .Select(Map).ToList();

    public async Task<SessionDto?> GetByIdAsync(Guid id)
    {
        var s = await _db.PaintSessions.Include(x => x.Owner).Include(x => x.Participants)
                         .FirstOrDefaultAsync(x => x.Id == id);
        return s is null ? null : Map(s);
    }

    public async Task DeleteAsync(Guid id, string uid)
    {
        var ses = await _db.PaintSessions.FindAsync(id);
        if (ses is null) return;
        if (ses.OwnerId != uid) throw new UnauthorizedAccessException();
        _db.PaintSessions.Remove(ses);
        await _db.SaveChangesAsync();
    }

    public async Task<List<StrokeDataDto>> GetStrokesAsync(Guid sessionId)
    {
        var records = await _db.StrokeRecords
            .Where(r => r.SessionId == sessionId).OrderBy(r => r.CreatedAt).ToListAsync();
        var result = new List<StrokeDataDto>();
        foreach (var r in records)
        {
            try
            {
                var dto = System.Text.Json.JsonSerializer.Deserialize<StrokeDataDto>(
                    r.StrokeDataJson,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (dto is not null) result.Add(dto);
            }
            catch { /* skip malformed */ }
        }
        return result;
    }

    public async Task SaveStrokeAsync(Guid sessionId, string userId, StrokeDataDto stroke)
    {
        _db.StrokeRecords.Add(new StrokeRecord
        {
            SessionId      = sessionId,
            UserId         = userId,
            StrokeDataJson = System.Text.Json.JsonSerializer.Serialize(stroke),
        });
        await _db.SaveChangesAsync();
    }

    public async Task SaveSnapshotAsync(Guid sessionId, string snapshot)
    {
        var ses = await _db.PaintSessions.FindAsync(sessionId);
        if (ses is null) return;
        ses.CanvasSnapshotBase64 = snapshot;
        await _db.SaveChangesAsync();
    }

    public async Task AddParticipantAsync(Guid sessionId, string userId)
    {
        if (await _db.SessionParticipants.AnyAsync(p => p.SessionId == sessionId && p.UserId == userId)) return;
        _db.SessionParticipants.Add(new SessionParticipant { SessionId = sessionId, UserId = userId });
        var ses = await _db.PaintSessions.FindAsync(sessionId);
        if (ses is not null) ses.IsCollaborative = true;
        await _db.SaveChangesAsync();
    }

    public async Task RemoveParticipantAsync(Guid sessionId, string userId)
    {
        var p = await _db.SessionParticipants.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.UserId == userId);
        if (p is null) return;
        _db.SessionParticipants.Remove(p);
        await _db.SaveChangesAsync();
    }
}
