using ChatApp.Core.Models;
using ChatApp.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Api.Services;

public class UserService
{
    private readonly AppDbContext _db;
    public UserService(AppDbContext db) => _db = db;

    public async Task<User?> FindByIdAsync(Guid id) =>
        await _db.Users.FindAsync(id);

    public async Task<User?> FindByUsernameAsync(string username) =>
        await _db.Users.FirstOrDefaultAsync(u => u.Username == username);

    public async Task<bool> ExistsByUsernameAsync(string username) =>
        await _db.Users.AnyAsync(u => u.Username == username);

    public async Task CreateAsync(User user) { _db.Users.Add(user); await _db.SaveChangesAsync(); }

    public async Task UpdateAsync(User user) { _db.Users.Update(user); await _db.SaveChangesAsync(); }

    public async Task SetOnlineAsync(Guid userId, bool online, DateTime? lastSeen = null)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return;
        user.IsOnline = online;
        if (lastSeen.HasValue) user.LastSeen = lastSeen.Value;
        await _db.SaveChangesAsync();
    }

    public async Task<List<User>> SearchAsync(string query, Guid excludeUserId, int limit) =>
        await _db.Users
            .Where(u => u.Id != excludeUserId &&
                (u.Username.ToLower().Contains(query.ToLower()) ||
                 u.DisplayName.ToLower().Contains(query.ToLower())))
            .Take(limit)
            .ToListAsync();
}
