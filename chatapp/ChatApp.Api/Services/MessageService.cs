using ChatApp.Api.DTOs;
using ChatApp.Core.Models;
using ChatApp.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Api.Services;

public class MessageService
{
    private readonly AppDbContext _db;
    public MessageService(AppDbContext db) => _db = db;

    private IQueryable<Message> WithIncludes() =>
        _db.Messages
            .Include(m => m.Sender)
            .Include(m => m.Attachments)
            .Include(m => m.Reactions).ThenInclude(r => r.User)
            .Include(m => m.ReplyTo).ThenInclude(r => r!.Sender);

    public async Task<Message> CreateAsync(SendMessageRequest req, Guid senderId)
    {
        if (!Enum.TryParse<MessageType>(req.Type, out var msgType))
            msgType = MessageType.Text;

        var msg = new Message
        {
            Id = Guid.NewGuid(), ChatId = req.ChatId, SenderId = senderId,
            Content = req.Content, Type = msgType,
            ReplyToId = req.ReplyToId, SentAt = DateTime.UtcNow
        };

        if (req.Attachments?.Any() == true)
        {
            msg.Attachments = req.Attachments.Select(a => new MessageAttachment
            {
                Id = Guid.NewGuid(), MessageId = msg.Id,
                Url = a.Url, FileName = a.FileName,
                FileSize = a.FileSize, MimeType = a.MimeType
            }).ToList();
        }

        _db.Messages.Add(msg);
        await _db.SaveChangesAsync();
        return await WithIncludes().FirstAsync(m => m.Id == msg.Id);
    }

    public async Task<Message?> EditAsync(Guid messageId, Guid userId, string content)
    {
        var msg = await _db.Messages.FindAsync(messageId);
        if (msg == null || msg.SenderId != userId || msg.IsDeleted) return null;
        msg.Content = content; msg.IsEdited = true; msg.EditedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await WithIncludes().FirstAsync(m => m.Id == messageId);
    }

    public async Task<Message?> DeleteAsync(Guid messageId, Guid userId)
    {
        var msg = await _db.Messages.FindAsync(messageId);
        if (msg == null || msg.SenderId != userId) return null;
        msg.IsDeleted = true; msg.Content = null;
        await _db.SaveChangesAsync();
        return msg;
    }

    public async Task<List<ReactionDto>> ToggleReactionAsync(Guid messageId, Guid userId, string emoji)
    {
        var existing = await _db.MessageReactions
            .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId && r.Emoji == emoji);

        if (existing != null) _db.MessageReactions.Remove(existing);
        else _db.MessageReactions.Add(new MessageReaction
        {
            MessageId = messageId, UserId = userId,
            Emoji = emoji, CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return await _db.MessageReactions
            .Where(r => r.MessageId == messageId)
            .Select(r => ReactionDto.From(r))
            .ToListAsync();
    }

    public async Task<Guid> GetChatIdAsync(Guid messageId)
    {
        var msg = await _db.Messages.FindAsync(messageId);
        return msg?.ChatId ?? Guid.Empty;
    }
}
