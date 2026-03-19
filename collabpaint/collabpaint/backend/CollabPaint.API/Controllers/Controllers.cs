using System.Security.Claims;
using CollabPaint.API.Data;
using CollabPaint.API.DTOs;
using CollabPaint.API.Models;
using CollabPaint.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollabPaint.API.Controllers;

// ── Users ───────────────────────────────────────────────────────────────────

[ApiController, Route("api/users"), Authorize]
public class UsersController : ControllerBase
{
    private readonly UserManager<AppUser> _users;
    private readonly AppDbContext         _db;

    public UsersController(UserManager<AppUser> users, AppDbContext db) { _users = users; _db = db; }

    private string Me => User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub")
                      ?? throw new UnauthorizedAccessException();

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMe()
    {
        var u = await _users.FindByIdAsync(Me);
        return u is null ? NotFound() : Ok(ToDto(u));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(string id)
    {
        var u = await _users.FindByIdAsync(id);
        return u is null ? NotFound() : Ok(ToDto(u));
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<UserDto>>> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return BadRequest(new { message = "Query must be at least 2 characters." });

        var lower = q.ToLower();
        var list = await _db.Users
            .Where(u => u.UserName!.ToLower().Contains(lower) || u.DisplayName.ToLower().Contains(lower))
            .Take(20).ToListAsync();
        return Ok(list.Select(ToDto).ToList());
    }

    private static UserDto ToDto(AppUser u) =>
        new(u.Id, u.UserName ?? "", u.Email ?? "", u.DisplayName, u.CreatedAt);
}

// ── Sessions ─────────────────────────────────────────────────────────────────

[ApiController, Route("api/sessions"), Authorize]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _svc;
    public SessionsController(ISessionService svc) => _svc = svc;

    private string Me => User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub")
                      ?? throw new UnauthorizedAccessException();

    [HttpPost]
    public async Task<ActionResult<SessionDto>> Create([FromBody] CreateSessionDto dto)
    {
        var s = await _svc.CreateAsync(Me, dto.Name);
        return CreatedAtAction(nameof(GetById), new { id = s.Id }, s);
    }

    [HttpGet]
    public async Task<ActionResult<List<SessionDto>>> GetAll() =>
        Ok(await _svc.GetAllForUserAsync(Me));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SessionDto>> GetById(Guid id)
    {
        var s = await _svc.GetByIdAsync(id);
        return s is null ? NotFound() : Ok(s);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try { await _svc.DeleteAsync(id, Me); return NoContent(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpGet("{id:guid}/strokes")]
    public async Task<ActionResult<List<StrokeDataDto>>> GetStrokes(Guid id) =>
        Ok(await _svc.GetStrokesAsync(id));

    [HttpPost("{id:guid}/snapshot")]
    public async Task<IActionResult> Snapshot(Guid id, [FromBody] SaveSnapshotDto dto)
    {
        await _svc.SaveSnapshotAsync(id, dto.SnapshotBase64);
        return NoContent();
    }
}
