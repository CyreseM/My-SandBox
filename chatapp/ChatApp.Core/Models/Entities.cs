namespace ChatApp.Core.Models;

public class User
{
    public Guid   Id           { get; set; }
    public string Username     { get; set; } = default!;
    public string DisplayName  { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string? AvatarUrl   { get; set; }
    public string? Bio         { get; set; }
    public DateTime LastSeen   { get; set; }
    public bool IsOnline       { get; set; }
    public ICollection<ChatMember> ChatMemberships { get; set; } = new List<ChatMember>();
    public ICollection<Status>     Statuses        { get; set; } = new List<Status>();
}

public class Chat
{
    public Guid    Id          { get; set; }
    public ChatType Type       { get; set; }
    public string? Name        { get; set; }
    public string? AvatarUrl   { get; set; }
    public string? Description { get; set; }
    public string? InviteToken { get; set; }
    public bool    IsPublic    { get; set; }
    public DateTime CreatedAt  { get; set; }
    public ICollection<ChatMember> Members  { get; set; } = new List<ChatMember>();
    public ICollection<Message>    Messages { get; set; } = new List<Message>();
}

public class ChatMember
{
    public Guid     ChatId      { get; set; }
    public Guid     UserId      { get; set; }
    public ChatRole Role        { get; set; }
    public DateTime JoinedAt    { get; set; }
    public DateTime? LastReadAt { get; set; }
    public bool IsMuted         { get; set; }
    public Chat Chat { get; set; } = default!;
    public User User { get; set; } = default!;
}

public class Message
{
    public Guid   Id        { get; set; }
    public Guid   ChatId    { get; set; }
    public Guid   SenderId  { get; set; }
    public string? Content  { get; set; }
    public MessageType Type { get; set; }
    public Guid?  ReplyToId { get; set; }
    public bool   IsEdited  { get; set; }
    public bool   IsDeleted { get; set; }
    public bool   IsPinned  { get; set; }
    public DateTime  SentAt   { get; set; }
    public DateTime? EditedAt { get; set; }
    public Message?  ReplyTo  { get; set; }
    public User      Sender   { get; set; } = default!;
    public Chat      Chat     { get; set; } = default!;
    public ICollection<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
    public ICollection<MessageReaction>   Reactions   { get; set; } = new List<MessageReaction>();
}

public class MessageAttachment
{
    public Guid   Id        { get; set; }
    public Guid   MessageId { get; set; }
    public string Url       { get; set; } = default!;
    public string FileName  { get; set; } = default!;
    public long   FileSize  { get; set; }
    public string MimeType  { get; set; } = default!;
    public int?   Width     { get; set; }
    public int?   Height    { get; set; }
    public Message Message  { get; set; } = default!;
}

public class MessageReaction
{
    public Guid   MessageId { get; set; }
    public Guid   UserId    { get; set; }
    public string Emoji     { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public User    User    { get; set; } = default!;
    public Message Message { get; set; } = default!;
}

public class Status
{
    public Guid   Id              { get; set; }
    public Guid   UserId          { get; set; }
    public string? Content        { get; set; }
    public string? MediaUrl       { get; set; }
    public StatusMediaType MediaType { get; set; }
    public string? BackgroundColor { get; set; }
    public DateTime CreatedAt     { get; set; }
    public DateTime ExpiresAt     { get; set; }
    public User                    User  { get; set; } = default!;
    public ICollection<StatusView> Views { get; set; } = new List<StatusView>();
}

public class StatusView
{
    public Guid     StatusId { get; set; }
    public Guid     ViewerId { get; set; }
    public DateTime ViewedAt { get; set; }
    public Status Status { get; set; } = default!;
    public User   Viewer { get; set; } = default!;
}

public enum ChatType        { Direct, Group, Channel }
public enum ChatRole        { Member, Admin, Owner }
public enum MessageType     { Text, Image, File, Voice }
public enum StatusMediaType { None, Image, Video }
