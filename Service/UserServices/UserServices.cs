using Core;
using DataLayer;
using Microsoft.EntityFrameworkCore;

namespace Service.UserServices;

public interface IUserServices
{
    Task<int> SaveUserAsync(AppUser user);
    Task<int> UpdateUserAsync(int userId, AppUser user);
    Task<AppUser?> GetUserAsync(int userId);
    Task<int> DeleteUserAsync(int userId);
    Task<IEnumerable<AppUser>?> GetAllUsersAsync();
    Task<AppUser?> GetUserWithLogsAsync(int userId);
    Task<IEnumerable<AppUser>?> GetUsersWithLogsAsync();
}

public class UserServices : IUserServices
{
    private readonly MasterDbContext _masterContext;

    public UserServices(MasterDbContext masterContext)
    {
        _masterContext = masterContext;
    }

    public async Task<int> DeleteUserAsync(int userId)
    {
        var user = await GetUserAsync(userId);
        if (user == null)
            throw new Exception("User not found");

        user.Deleted = true;
        _masterContext.AppUser.Update(user);

        await _masterContext.SaveChangesAsync();

        return user.AppUserId;
    }

    public async Task<IEnumerable<AppUser>?> GetAllUsersAsync()
    {
        return await _masterContext.AppUser
            .ToListAsync();
    }

    public async Task<AppUser?> GetUserAsync(int userId)
    {
        return await _masterContext.AppUser
            .FirstOrDefaultAsync(x => x.AppUserId == userId);
    }

    public async Task<IEnumerable<AppUser>?> GetUsersWithLogsAsync()
    {
        return await _masterContext.AppUser.Include(au => au.SystemLogs).ToListAsync();
    }

    public async Task<AppUser?> GetUserWithLogsAsync(int userId)
    {
        return await _masterContext.AppUser.Include(au => au.SystemLogs).FirstOrDefaultAsync(q => q.AppUserId == userId);
    }

    public async Task<int> SaveUserAsync(AppUser user)
    {
        await _masterContext.AddAsync(user);
        await _masterContext.SaveChangesAsync();
        return user.AppUserId;
    }

    public async Task<int> UpdateUserAsync(int userId, AppUser user)
    {
        var dbUser = await GetUserAsync(userId);
        if (user == null)
            throw new Exception("User not found");

        dbUser.Username = user.Username;
        dbUser.EmailAddress = user.EmailAddress;
        dbUser.Password = user.Password;

        _masterContext.AppUser.Update(dbUser);

        await _masterContext.SaveChangesAsync();

        return dbUser.AppUserId;
    }
}