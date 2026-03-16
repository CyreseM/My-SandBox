using ChatApp.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User>              Users              => Set<User>();
    public DbSet<Chat>              Chats              => Set<Chat>();
    public DbSet<ChatMember>        ChatMembers        => Set<ChatMember>();
    public DbSet<Message>           Messages           => Set<Message>();
    public DbSet<MessageAttachment> MessageAttachments => Set<MessageAttachment>();
    public DbSet<MessageReaction>   MessageReactions   => Set<MessageReaction>();
    public DbSet<Status>            Statuses           => Set<Status>();
    public DbSet<StatusView>        StatusViews        => Set<StatusView>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<ChatMember>()    .HasKey(m => new { m.ChatId, m.UserId });
        b.Entity<MessageReaction>().HasKey(r => new { r.MessageId, r.UserId });
        b.Entity<StatusView>()    .HasKey(v => new { v.StatusId, v.ViewerId });

        b.Entity<Message>()
            .HasOne(m => m.ReplyTo)
            .WithMany()
            .HasForeignKey(m => m.ReplyToId)
            .OnDelete(DeleteBehavior.SetNull);

        b.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<ChatMember>()
            .HasOne(cm => cm.User)
            .WithMany(u => u.ChatMemberships)
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<ChatMember>()
            .HasOne(cm => cm.Chat)
            .WithMany(c => c.Members)
            .HasForeignKey(cm => cm.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<MessageReaction>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<MessageReaction>()
            .HasOne(r => r.Message)
            .WithMany(m => m.Reactions)
            .HasForeignKey(r => r.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<StatusView>()
            .HasOne(sv => sv.Viewer)
            .WithMany()
            .HasForeignKey(sv => sv.ViewerId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<StatusView>()
            .HasOne(sv => sv.Status)
            .WithMany(s => s.Views)
            .HasForeignKey(sv => sv.StatusId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<User>().HasIndex(u => u.Username).IsUnique();

        // Convert enums to strings for readability
        b.Entity<Chat>().Property(c => c.Type).HasConversion<string>();
        b.Entity<ChatMember>().Property(cm => cm.Role).HasConversion<string>();
        b.Entity<Message>().Property(m => m.Type).HasConversion<string>();
        b.Entity<Status>().Property(s => s.MediaType).HasConversion<string>();
    }
}
