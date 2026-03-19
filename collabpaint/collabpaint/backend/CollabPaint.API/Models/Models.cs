using Microsoft.AspNetCore.Identity;

namespace CollabPaint.API.Models;

public class AppUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<PaintSession> OwnedSessions { get; set; }     = new List<PaintSession>();
    public ICollection<SessionParticipant> Participations { get; set; } = new List<SessionParticipant>();
}

public class PaintSession
{
    public Guid   Id                      { get; set; } = Guid.NewGuid();
    public string Name                    { get; set; } = string.Empty;
    public string OwnerId                 { get; set; } = string.Empty;
    public AppUser? Owner                 { get; set; }
    public bool   IsCollaborative         { get; set; } = false;
    public DateTime CreatedAt             { get; set; } = DateTime.UtcNow;
    public string? CanvasSnapshotBase64   { get; set; }
    public ICollection<SessionParticipant> Participants { get; set; } = new List<SessionParticipant>();
    public ICollection<StrokeRecord>       Strokes      { get; set; } = new List<StrokeRecord>();
}

public class SessionParticipant
{
    public Guid   Id        { get; set; } = Guid.NewGuid();
    public Guid   SessionId { get; set; }
    public PaintSession? Session { get; set; }
    public string UserId   { get; set; } = string.Empty;
    public AppUser? User   { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

public class StrokeRecord
{
    public Guid   Id             { get; set; } = Guid.NewGuid();
    public Guid   SessionId      { get; set; }
    public PaintSession? Session { get; set; }
    public string UserId         { get; set; } = string.Empty;
    public string StrokeDataJson { get; set; } = string.Empty;
    public DateTime CreatedAt    { get; set; } = DateTime.UtcNow;
}
