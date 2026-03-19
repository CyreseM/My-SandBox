namespace CollabPaint.API.DTOs;

public record RegisterDto(string Username, string Email, string Password, string DisplayName);
public record LoginDto(string Email, string Password);
public record AuthResponseDto(string Token, string UserId, string Username, string DisplayName);

public record UserDto(string Id, string Username, string Email, string DisplayName, DateTime CreatedAt);

public record CreateSessionDto(string Name);
public record SessionDto(Guid Id, string Name, string OwnerId, string OwnerName, bool IsCollaborative,
                         DateTime CreatedAt, int ParticipantCount, string? CanvasSnapshotBase64);
public record SaveSnapshotDto(string SnapshotBase64);

public record PointDto(double X, double Y);
public record StrokeDataDto(string Tool, string Color, double LineWidth, List<PointDto> Points,
                             DateTime Timestamp, string? UserId, string? Text, bool FillShape);

public record CursorPayload(string UserId, string Username, double X, double Y, string Color);
public record UserJoinedPayload(string UserId, string Username, string DisplayName, string Color);
public record InvitePayload(string FromUserId, string FromUsername, string SessionId, string SessionName);
