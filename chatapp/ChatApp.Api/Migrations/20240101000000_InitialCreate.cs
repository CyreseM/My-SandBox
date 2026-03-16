using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chats",
                columns: table => new
                {
                    Id          = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type        = table.Column<string>(type: "TEXT", nullable: false),
                    Name        = table.Column<string>(type: "TEXT", nullable: true),
                    AvatarUrl   = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    InviteToken = table.Column<string>(type: "TEXT", nullable: true),
                    IsPublic    = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt   = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Chats", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id           = table.Column<Guid>(type: "TEXT", nullable: false),
                    Username     = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName  = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    AvatarUrl    = table.Column<string>(type: "TEXT", nullable: true),
                    Bio          = table.Column<string>(type: "TEXT", nullable: true),
                    LastSeen     = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsOnline     = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Users", x => x.Id));

            migrationBuilder.CreateTable(
                name: "ChatMembers",
                columns: table => new
                {
                    ChatId     = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId     = table.Column<Guid>(type: "TEXT", nullable: false),
                    Role       = table.Column<string>(type: "TEXT", nullable: false),
                    JoinedAt   = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastReadAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsMuted    = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMembers", x => new { x.ChatId, x.UserId });
                    table.ForeignKey("FK_ChatMembers_Chats_ChatId", x => x.ChatId, "Chats", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_ChatMembers_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id        = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChatId    = table.Column<Guid>(type: "TEXT", nullable: false),
                    SenderId  = table.Column<Guid>(type: "TEXT", nullable: false),
                    Content   = table.Column<string>(type: "TEXT", nullable: true),
                    Type      = table.Column<string>(type: "TEXT", nullable: false),
                    ReplyToId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsEdited  = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsPinned  = table.Column<bool>(type: "INTEGER", nullable: false),
                    SentAt    = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EditedAt  = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey("FK_Messages_Chats_ChatId",      x => x.ChatId,    "Chats",    "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_Messages_Users_SenderId",    x => x.SenderId,  "Users",    "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_Messages_Messages_ReplyToId",x => x.ReplyToId, "Messages", "Id", onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MessageAttachments",
                columns: table => new
                {
                    Id        = table.Column<Guid>(type: "TEXT", nullable: false),
                    MessageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Url       = table.Column<string>(type: "TEXT", nullable: false),
                    FileName  = table.Column<string>(type: "TEXT", nullable: false),
                    FileSize  = table.Column<long>(type: "INTEGER", nullable: false),
                    MimeType  = table.Column<string>(type: "TEXT", nullable: false),
                    Width     = table.Column<int>(type: "INTEGER", nullable: true),
                    Height    = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageAttachments", x => x.Id);
                    table.ForeignKey("FK_MessageAttachments_Messages_MessageId", x => x.MessageId, "Messages", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MessageReactions",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId    = table.Column<Guid>(type: "TEXT", nullable: false),
                    Emoji     = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageReactions", x => new { x.MessageId, x.UserId });
                    table.ForeignKey("FK_MessageReactions_Messages_MessageId", x => x.MessageId, "Messages", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_MessageReactions_Users_UserId",       x => x.UserId,    "Users",    "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Statuses",
                columns: table => new
                {
                    Id              = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId          = table.Column<Guid>(type: "TEXT", nullable: false),
                    Content         = table.Column<string>(type: "TEXT", nullable: true),
                    MediaUrl        = table.Column<string>(type: "TEXT", nullable: true),
                    MediaType       = table.Column<string>(type: "TEXT", nullable: false),
                    BackgroundColor = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt       = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAt       = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statuses", x => x.Id);
                    table.ForeignKey("FK_Statuses_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatusViews",
                columns: table => new
                {
                    StatusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ViewerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusViews", x => new { x.StatusId, x.ViewerId });
                    table.ForeignKey("FK_StatusViews_Statuses_StatusId", x => x.StatusId, "Statuses", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_StatusViews_Users_ViewerId",    x => x.ViewerId,  "Users",    "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_Users_Username",         "Users",        "Username", unique: true);
            migrationBuilder.CreateIndex("IX_ChatMembers_UserId",     "ChatMembers",  "UserId");
            migrationBuilder.CreateIndex("IX_Messages_ChatId",        "Messages",     "ChatId");
            migrationBuilder.CreateIndex("IX_Messages_SenderId",      "Messages",     "SenderId");
            migrationBuilder.CreateIndex("IX_Messages_ReplyToId",     "Messages",     "ReplyToId");
            migrationBuilder.CreateIndex("IX_MessageAttachments_Mid", "MessageAttachments", "MessageId");
            migrationBuilder.CreateIndex("IX_MessageReactions_UserId","MessageReactions",   "UserId");
            migrationBuilder.CreateIndex("IX_Statuses_UserId",        "Statuses",     "UserId");
            migrationBuilder.CreateIndex("IX_StatusViews_ViewerId",   "StatusViews",  "ViewerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("StatusViews");
            migrationBuilder.DropTable("Statuses");
            migrationBuilder.DropTable("MessageReactions");
            migrationBuilder.DropTable("MessageAttachments");
            migrationBuilder.DropTable("Messages");
            migrationBuilder.DropTable("ChatMembers");
            migrationBuilder.DropTable("Users");
            migrationBuilder.DropTable("Chats");
        }
    }
}
