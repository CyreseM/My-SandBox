using CollabPaint.API.DTOs;
using CollabPaint.API.Models;
using CollabPaint.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CollabPaint.API.Controllers;

[ApiController, Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser>   _users;
    private readonly SignInManager<AppUser> _signIn;
    private readonly ITokenService          _tokens;

    public AuthController(UserManager<AppUser> users, SignInManager<AppUser> signIn, ITokenService tokens)
    { _users = users; _signIn = signIn; _tokens = tokens; }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        if (await _users.FindByEmailAsync(dto.Email) is not null)
            return BadRequest(new { message = "Email already in use." });
        if (await _users.FindByNameAsync(dto.Username) is not null)
            return BadRequest(new { message = "Username already taken." });

        var user = new AppUser { UserName = dto.Username, Email = dto.Email, DisplayName = dto.DisplayName };
        var result = await _users.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(new { message = string.Join("; ", result.Errors.Select(e => e.Description)) });

        return Ok(new AuthResponseDto(_tokens.CreateToken(user), user.Id, user.UserName!, user.DisplayName));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var user = await _users.FindByEmailAsync(dto.Email);
        if (user is null) return Unauthorized(new { message = "Invalid credentials." });

        var ok = await _signIn.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!ok.Succeeded) return Unauthorized(new { message = "Invalid credentials." });

        return Ok(new AuthResponseDto(_tokens.CreateToken(user), user.Id, user.UserName!, user.DisplayName));
    }
}
