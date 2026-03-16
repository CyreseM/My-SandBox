using ChatApp.Core.Models;

namespace ChatApp.Api.DTOs;

// Auth
public record LoginDto(string Username, string Password);
public record RegisterDto(string Username, string DisplayName, string Password);

// User
public record UserDto(
    Guid Id,
    string Username,
    string DisplayName,
    string? AvatarUrl,
    string? Bio,
    bool IsOnline,
    DateTime LastSeen)
{
    public static UserDto From(User u) =>
        new(u.Id, u.Username, u.DisplayName, u.AvatarUrl, u.Bio, u.IsOnline, u.LastSeen);
}

public record UpdateProfileDto(string Username, string DisplayName, string? Bio);

// Chat
public record ChatDto(
    Guid Id,
    string Type,
    string? Name,
    string? AvatarUrl,
    string? Description,
    bool IsPublic,
    int MemberCount,
    MessageDto? LastMessage,
    int UnreadCount,
    Guid? OtherUserId,
    string? OtherUserName,
    DateTime CreatedAt)
{
    public static ChatDto From(Chat c, Guid currentUserId, int unread = 0)
    {
        Guid? otherId = null;
        string? otherName = null;
        if (c.Type == ChatType.Direct)
        {
            var other = c.Members.FirstOrDefault(m => m.UserId != currentUserId);
            otherId   = other?.UserId;
            otherName = other?.User?.DisplayName;
        }
        var lastMsg = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
        return new ChatDto(
            c.Id, c.Type.ToString(), c.Type == ChatType.Direct ? otherName ?? "Direct" : c.Name,
            c.AvatarUrl, c.Description, c.IsPublic,
            c.Members.Count, lastMsg != null ? MessageDto.From(lastMsg, currentUserId) : null,
            unread, otherId, otherName, c.CreatedAt);
    }
}

public record CreateDirectDto(Guid UserId);
public record CreateGroupDto(string Name, string? Description, List<Guid> MemberIds);
public record CreateChannelDto(string Name, string? Description, bool IsPublic);
public record AddMemberDto(Guid UserId);
public record ChangeRoleDto(string Role);

// Message
public record MessageDto(
    Guid Id,
    Guid ChatId,
    Guid SenderId,
    string SenderDisplayName,
    string? SenderAvatarUrl,
    string? Content,
    string Type,
    bool IsEdited,
    bool IsDeleted,
    bool IsPinned,
    DateTime SentAt,
    Guid? ReplyToId,
    MessageDto? ReplyTo,
    List<AttachmentDto> Attachments,
    List<ReactionDto> Reactions)
{
    public static MessageDto From(Message m, Guid currentUserId)
    {
        if (m.IsDeleted)
            return new MessageDto(m.Id, m.ChatId, m.SenderId,
                m.Sender?.DisplayName ?? "", m.Sender?.AvatarUrl,
                null, m.Type.ToString(), m.IsEdited, true, m.IsPinned, m.SentAt,
                null, null, new(), new());

        return new MessageDto(
            m.Id, m.ChatId, m.SenderId,
            m.Sender?.DisplayName ?? "", m.Sender?.AvatarUrl,
            m.Content, m.Type.ToString(), m.IsEdited, m.IsDeleted, m.IsPinned, m.SentAt,
            m.ReplyToId,
            m.ReplyTo != null ? From(m.ReplyTo, currentUserId) : null,
            m.Attachments?.Select(AttachmentDto.From).ToList() ?? new(),
            m.Reactions?.Select(ReactionDto.From).ToList() ?? new());
    }
}

public record AttachmentDto(Guid Id, string Url, string FileName, long FileSize, string MimeType)
{
    public static AttachmentDto From(MessageAttachment a) =>
        new(a.Id, a.Url, a.FileName, a.FileSize, a.MimeType);
}

public record ReactionDto(Guid MessageId, Guid UserId, string Emoji, DateTime CreatedAt)
{
    public static ReactionDto From(MessageReaction r) =>
        new(r.MessageId, r.UserId, r.Emoji, r.CreatedAt);
}

public record SendMessageRequest(
    Guid ChatId,
    string? Content,
    string Type = "Text",
    Guid? ReplyToId = null,
    List<AttachmentDto>? Attachments = null);

// Status
public record CreateStatusDto(
    string? Content,
    string? MediaUrl,
    string MediaType = "None",
    string? BackgroundColor = null);

public record StatusDto(
    Guid Id,
    Guid UserId,
    string SenderDisplayName,
    string? SenderAvatarUrl,
    string? Content,
    string? MediaUrl,
    string MediaType,
    string? BackgroundColor,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    int ViewCount,
    bool IsOwn)
{
    public static StatusDto From(Status s, Guid currentUserId) =>
        new(s.Id, s.UserId,
            s.User?.DisplayName ?? "", s.User?.AvatarUrl,
            s.Content, s.MediaUrl, s.MediaType.ToString(), s.BackgroundColor,
            s.CreatedAt, s.ExpiresAt, s.Views?.Count ?? 0, s.UserId == currentUserId);
}

public record StatusGroupDto(
    Guid UserId,
    string DisplayName,
    string? AvatarUrl,
    List<Guid> StatusIds,
    bool HasUnviewed);

// Member
public record MemberDto(Guid UserId, string Role, DateTime JoinedAt, UserDto User)
{
    public static MemberDto From(ChatMember m) =>
        new(m.UserId, m.Role.ToString(), m.JoinedAt, UserDto.From(m.User));
}

// Messages page
public record MessagesPage(List<MessageDto> Messages, bool HasMore, string? Cursor);
