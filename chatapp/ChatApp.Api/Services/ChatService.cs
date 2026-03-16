using ChatApp.Api.DTOs;
using ChatApp.Core.Models;
using ChatApp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace ChatApp.Api.Services;

public class ChatService
{
    private readonly AppDbContext _db;
    public ChatService(AppDbContext db) => _db = db;

    private IQueryable<Chat> ChatsWithIncludes() =>
        _db.Chats
            .Include(c => c.Members).ThenInclude(m => m.User)
            .Include(c => c.Messages)
                .ThenInclude(m => m.Sender);

    public async Task<List<ChatDto>> GetUserChatsAsync(Guid userId)
    {
        var memberChatIds = await _db.ChatMembers
            .Where(m => m.UserId == userId)
            .Select(m => m.ChatId)
            .ToListAsync();

        var chats = await ChatsWithIncludes()
            .Where(c => memberChatIds.Contains(c.Id))
            .ToListAsync();

        return chats
            .OrderByDescending(c => c.Messages.FirstOrDefault()?.SentAt ?? c.CreatedAt)
            .Select(c => ChatDto.From(c, userId))
            .ToList();
    }

    public async Task<Chat?> GetChatAsync(Guid chatId, Guid userId)
    {
        return await ChatsWithIncludes()
            .FirstOrDefaultAsync(c => c.Id == chatId &&
                c.Members.Any(m => m.UserId == userId));
    }

    public async Task<Chat> GetOrCreateDirectAsync(Guid userId, Guid targetUserId)
    {
        // Check for existing DM
        var existing = await _db.Chats
            .Include(c => c.Members).ThenInclude(m => m.User)
            .Include(c => c.Messages)
                .ThenInclude(m => m.Sender)
            .FirstOrDefaultAsync(c => c.Type == ChatType.Direct &&
                c.Members.Any(m => m.UserId == userId) &&
                c.Members.Any(m => m.UserId == targetUserId));

        if (existing != null) return existing;

        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Type = ChatType.Direct,
            CreatedAt = DateTime.UtcNow
        };
        _db.Chats.Add(chat);

        _db.ChatMembers.AddRange(
            new ChatMember { ChatId = chat.Id, UserId = userId,       Role = ChatRole.Member, JoinedAt = DateTime.UtcNow },
            new ChatMember { ChatId = chat.Id, UserId = targetUserId, Role = ChatRole.Member, JoinedAt = DateTime.UtcNow }
        );

        await _db.SaveChangesAsync();

        return await ChatsWithIncludes().FirstAsync(c => c.Id == chat.Id);
    }

    public async Task<Chat> CreateGroupAsync(string name, string? description, List<Guid> memberIds, Guid ownerId)
    {
        var chat = new Chat
        {
            Id = Guid.NewGuid(), Type = ChatType.Group,
            Name = name, Description = description, CreatedAt = DateTime.UtcNow
        };
        _db.Chats.Add(chat);

        var allIds = memberIds.Union(new[] { ownerId }).Distinct();
        foreach (var uid in allIds)
        {
            _db.ChatMembers.Add(new ChatMember
            {
                ChatId = chat.Id, UserId = uid,
                Role = uid == ownerId ? ChatRole.Owner : ChatRole.Member,
                JoinedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
        return await ChatsWithIncludes().FirstAsync(c => c.Id == chat.Id);
    }

    public async Task<Chat> CreateChannelAsync(string name, string? description, bool isPublic, Guid ownerId)
    {
        var chat = new Chat
        {
            Id = Guid.NewGuid(), Type = ChatType.Channel, Name = name,
            Description = description, IsPublic = isPublic,
            InviteToken = GenerateToken(), CreatedAt = DateTime.UtcNow
        };
        _db.Chats.Add(chat);
        _db.ChatMembers.Add(new ChatMember
        {
            ChatId = chat.Id, UserId = ownerId,
            Role = ChatRole.Owner, JoinedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return await ChatsWithIncludes().FirstAsync(c => c.Id == chat.Id);
    }

    public async Task<bool> IsMemberAsync(Guid chatId, Guid userId) =>
        await _db.ChatMembers.AnyAsync(m => m.ChatId == chatId && m.UserId == userId);

    public async Task<bool> CanSendMessageAsync(Guid chatId, Guid userId)
    {
        var member = await _db.ChatMembers
            .Include(m => m.Chat)
            .FirstOrDefaultAsync(m => m.ChatId == chatId && m.UserId == userId);
        if (member == null) return false;
        if (member.Chat.Type == ChatType.Channel)
            return member.Role is ChatRole.Admin or ChatRole.Owner;
        return true;
    }

    public async Task<List<Guid>> GetUserChatIdsAsync(Guid userId) =>
        await _db.ChatMembers.Where(m => m.UserId == userId).Select(m => m.ChatId).ToListAsync();

    public async Task<MessagesPage> GetMessagesPageAsync(Guid chatId, string? before, int limit)
    {
        var query = _db.Messages
            .Where(m => m.ChatId == chatId)
            .Include(m => m.Sender)
            .Include(m => m.Attachments)
            .Include(m => m.Reactions).ThenInclude(r => r.User)
            .Include(m => m.ReplyTo).ThenInclude(r => r!.Sender)
            .OrderByDescending(m => m.SentAt);

        if (!string.IsNullOrEmpty(before) && DateTime.TryParse(before, out var beforeDate))
            query = (IOrderedQueryable<Message>)query.Where(m => m.SentAt < beforeDate);

        var messages = await query.Take(limit + 1).ToListAsync();
        var hasMore = messages.Count > limit;
        if (hasMore) messages = messages.Take(limit).ToList();

        var cursor = messages.LastOrDefault()?.SentAt.ToString("O");
        return new MessagesPage(
            messages.Select(m => MessageDto.From(m, Guid.Empty)).ToList(),
            hasMore, cursor);
    }

    public async Task<List<MemberDto>> GetMembersAsync(Guid chatId) =>
        await _db.ChatMembers
            .Where(m => m.ChatId == chatId)
            .Include(m => m.User)
            .Select(m => MemberDto.From(m))
            .ToListAsync();

    public async Task AddMemberAsync(Guid chatId, Guid userId)
    {
        if (!await _db.ChatMembers.AnyAsync(m => m.ChatId == chatId && m.UserId == userId))
        {
            _db.ChatMembers.Add(new ChatMember
            {
                ChatId = chatId, UserId = userId,
                Role = ChatRole.Member, JoinedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }
    }

    public async Task RemoveMemberAsync(Guid chatId, Guid userId)
    {
        var m = await _db.ChatMembers.FindAsync(chatId, userId);
        if (m != null) { _db.ChatMembers.Remove(m); await _db.SaveChangesAsync(); }
    }

    public async Task ChangeRoleAsync(Guid chatId, Guid userId, string role)
    {
        var m = await _db.ChatMembers.FindAsync(chatId, userId);
        if (m != null && Enum.TryParse<ChatRole>(role, out var r))
        {
            m.Role = r;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<string> RegenerateInviteTokenAsync(Guid chatId)
    {
        var chat = await _db.Chats.FindAsync(chatId);
        if (chat == null) throw new Exception("Chat not found");
        chat.InviteToken = GenerateToken();
        await _db.SaveChangesAsync();
        return chat.InviteToken;
    }

    public async Task<Chat?> JoinViaTokenAsync(string token, Guid userId)
    {
        var chat = await ChatsWithIncludes().FirstOrDefaultAsync(c => c.InviteToken == token);
        if (chat == null) return null;
        await AddMemberAsync(chat.Id, userId);
        return await ChatsWithIncludes().FirstAsync(c => c.Id == chat.Id);
    }

    public async Task UpdateLastReadAsync(Guid chatId, Guid userId, DateTime readAt)
    {
        var m = await _db.ChatMembers.FindAsync(chatId, userId);
        if (m != null) { m.LastReadAt = readAt; await _db.SaveChangesAsync(); }
    }

    public async Task<List<Guid>> GetDirectPartnerIdsAsync(Guid userId)
    {
        var directChatIds = await _db.ChatMembers
            .Where(m => m.UserId == userId)
            .Join(_db.Chats.Where(c => c.Type == ChatType.Direct),
                  m => m.ChatId, c => c.Id, (m, c) => c.Id)
            .ToListAsync();

        return await _db.ChatMembers
            .Where(m => directChatIds.Contains(m.ChatId) && m.UserId != userId)
            .Select(m => m.UserId)
            .Distinct()
            .ToListAsync();
    }

    private static string GenerateToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(18))
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');
}
