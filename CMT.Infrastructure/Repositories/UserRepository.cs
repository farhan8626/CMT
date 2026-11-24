// ============================================================
//  CMT.Infrastructure – User Repository (EF Core)
// ============================================================
using CMT.Core.DTOs.Auth;
using CMT.Core.Entities;
using CMT.Core.Interfaces;
using CMT.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CMT.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly CmtDbContext _db;
    public UserRepository(CmtDbContext db) => _db = db;

    public async Task<User?> GetByEmailAsync(string email) =>
        await _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower());

    public async Task<User?> GetByIdAsync(int id) =>
        await _db.Users.FindAsync(id);

    public async Task<IEnumerable<UserDto>> GetAllAsync() =>
        await _db.Users
            .OrderBy(u => u.FullName)
            .Select(u => new UserDto(u.Id, u.FullName, u.Email, u.Role, u.IsActive, u.CreatedAt))
            .AsNoTracking()
            .ToListAsync();

    public async Task<User> CreateAsync(User user)
    {
        user.Email = user.Email.ToLower();
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        var q = _db.Users.Where(u => u.Email == email.ToLower());
        if (excludeId.HasValue) q = q.Where(u => u.Id != excludeId.Value);
        return await q.AnyAsync();
    }
}
