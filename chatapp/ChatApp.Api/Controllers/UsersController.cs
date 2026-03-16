using System.Security.Claims;
using ChatApp.Api.DTOs;
using ChatApp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Api.Controllers;

[ApiController, Route("api/users"), Authorize]
public class UsersController : ControllerBase
{
    private readonly UserService _users;
    private readonly FileStorageService _files;
    public UsersController(UserService u, FileStorageService f) => (_users, _files) = (u, f);

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Ok(Array.Empty<UserDto>());
        var results = await _users.SearchAsync(q.Trim(), GetUserId(), limit);
        return Ok(results.Select(UserDto.From));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _users.FindByIdAsync(id);
        return user == null ? NotFound() : Ok(UserDto.From(user));
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var user = await _users.FindByIdAsync(GetUserId());
        if (user == null) return NotFound();
        if (dto.Username != user.Username && await _users.ExistsByUsernameAsync(dto.Username))
            return Conflict(new { field = "username", message = "Username already taken" });
        user.DisplayName = dto.DisplayName;
        user.Username    = dto.Username;
        user.Bio         = dto.Bio;
        await _users.UpdateAsync(user);
        return Ok(UserDto.From(user));
    }

    [HttpPost("me/avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile avatar)
    {
        var url  = await _files.UploadAsync(avatar);
        var user = await _users.FindByIdAsync(GetUserId());
        if (user == null) return NotFound();
        user.AvatarUrl = url;
        await _users.UpdateAsync(user);
        return Ok(new { avatarUrl = url });
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

[ApiController, Route("api/files"), Authorize]
public class FilesController : ControllerBase
{
    private readonly FileStorageService _files;
    public FilesController(FileStorageService f) => _files = f;

    [HttpPost("upload"), RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        var url = await _files.UploadAsync(file);
        return Ok(new
        {
            url,
            fileName = file.FileName,
            mimeType = file.ContentType,
            size     = file.Length
        });
    }
}
