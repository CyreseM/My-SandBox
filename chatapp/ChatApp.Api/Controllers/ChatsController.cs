using System.Security.Claims;
using ChatApp.Api.DTOs;
using ChatApp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Api.Controllers;

[ApiController, Route("api/chats"), Authorize]
public class ChatsController : ControllerBase
{
    private readonly ChatService _chats;
    public ChatsController(ChatService chats) => _chats = chats;

    [HttpGet]
    public async Task<IActionResult> List() =>
        Ok(await _chats.GetUserChatsAsync(GetUserId()));

    [HttpGet("{chatId}")]
    public async Task<IActionResult> Get(Guid chatId)
    {
        var chat = await _chats.GetChatAsync(chatId, GetUserId());
        return chat == null ? NotFound() : Ok(ChatDto.From(chat, GetUserId()));
    }

    [HttpPost("direct")]
    public async Task<IActionResult> CreateDirect([FromBody] CreateDirectDto dto)
    {
        var chat = await _chats.GetOrCreateDirectAsync(GetUserId(), dto.UserId);
        return Ok(ChatDto.From(chat, GetUserId()));
    }

    [HttpPost("group")]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDto dto)
    {
        var chat = await _chats.CreateGroupAsync(dto.Name, dto.Description, dto.MemberIds, GetUserId());
        return Ok(ChatDto.From(chat, GetUserId()));
    }

    [HttpPost("channel")]
    public async Task<IActionResult> CreateChannel([FromBody] CreateChannelDto dto)
    {
        var chat = await _chats.CreateChannelAsync(dto.Name, dto.Description, dto.IsPublic, GetUserId());
        return Ok(ChatDto.From(chat, GetUserId()));
    }

    [HttpGet("{chatId}/messages")]
    public async Task<IActionResult> GetMessages(
        Guid chatId, [FromQuery] string? before, [FromQuery] int limit = 50)
    {
        if (!await _chats.IsMemberAsync(chatId, GetUserId())) return Forbid();
        return Ok(await _chats.GetMessagesPageAsync(chatId, before, limit));
    }

    [HttpGet("{chatId}/members")]
    public async Task<IActionResult> GetMembers(Guid chatId)
    {
        if (!await _chats.IsMemberAsync(chatId, GetUserId())) return Forbid();
        return Ok(await _chats.GetMembersAsync(chatId));
    }

    [HttpPost("{chatId}/members")]
    public async Task<IActionResult> AddMember(Guid chatId, [FromBody] AddMemberDto dto)
    {
        await _chats.AddMemberAsync(chatId, dto.UserId); return NoContent();
    }

    [HttpDelete("{chatId}/members/{userId}")]
    public async Task<IActionResult> RemoveMember(Guid chatId, Guid userId)
    {
        await _chats.RemoveMemberAsync(chatId, userId); return NoContent();
    }

    [HttpPut("{chatId}/members/{userId}/role")]
    public async Task<IActionResult> ChangeRole(Guid chatId, Guid userId, [FromBody] ChangeRoleDto dto)
    {
        await _chats.ChangeRoleAsync(chatId, userId, dto.Role); return NoContent();
    }

    [HttpPost("{chatId}/invite")]
    public async Task<IActionResult> GenerateInvite(Guid chatId)
    {
        var token = await _chats.RegenerateInviteTokenAsync(chatId);
        return Ok(new { link = $"{Request.Scheme}://{Request.Host}/join/{token}" });
    }

    [HttpGet("join/{token}")]
    public async Task<IActionResult> JoinViaLink(string token)
    {
        var chat = await _chats.JoinViaTokenAsync(token, GetUserId());
        return chat == null ? NotFound() : Ok(ChatDto.From(chat, GetUserId()));
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
