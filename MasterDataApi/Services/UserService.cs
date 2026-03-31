using Microsoft.EntityFrameworkCore;
using MasterDataApi.Data;
using MasterDataApi.DTOs;
using MasterDataApi.Models;

namespace MasterDataApi.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<UserResponse>> GetAllAsync()
    {
        return await _db.Users
            .OrderBy(u => u.FullName)
            .Select(u => MapToResponse(u))
            .ToListAsync();
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        return user == null ? null : MapToResponse(user);
    }

    public async Task<(UserResponse? result, string? errorCode)> CreateAsync(CreateUserRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var emailExists = await _db.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
        if (emailExists) return (null, "DUPLICATE_USER_EMAIL");

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return (MapToResponse(user), null);
    }

    public async Task<(UserResponse? result, string? errorCode)> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return (null, "USER_NOT_FOUND");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var emailExists = await _db.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail && u.Id != id);
        if (emailExists) return (null, "DUPLICATE_USER_EMAIL");

        user.FullName = request.FullName.Trim();
        user.Email = normalizedEmail;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (MapToResponse(user), null);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return false;

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return true;
    }

    private static UserResponse MapToResponse(User u) => new()
    {
        Id = u.Id,
        FullName = u.FullName,
        Email = u.Email,
        CreatedAt = u.CreatedAt,
        UpdatedAt = u.UpdatedAt
    };
}
