using Core;
using DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Service.LogServices;

public interface ILogServices
{
    Task<int> SaveLogAsync(SystemLog systemLog);
    Task<EntityEntry> GetEntryAsync(int logId);
    Task<int> UpdateLogAsync(int logId, SystemLog systemLog);
    Task<int> CountUserLogsAsync(int userId);
    Task<bool> CheckLogExistsAsync(int logId);
    Task<int> DeleteLogAsync(int logId);
    Task<SystemLog?> GetLogByIdWithFindCmdAsync(int logId);
    Task<SystemLog?> GetLogByIdWithSingleCmdAsync(int logId);
    Task<SystemLog?> GetLogByIdWithFirstCmdAsync(int logId);
    Task<bool> ExecuteDeleteSqlRawAsync(int logId);
    Task<bool> ExecuteDeleteSqlInterpolatedAsync(int logId);
    Task<List<SystemLog>> GetLogsAsync();
    Task<List<dynamic>> GetAnonymousLogDataList();
    Task<int> ReadMaxLogIdAsync();
    Task<int> ReadMinLogIdAsync();
    Task<double> ReadAverageLogIdAsync();
    Task<List<dynamic>> ReadGroupedLogsAsync();
    Task<List<dynamic>> ReadJoinedLogsAsync();
}

public class LogServices : ILogServices
{
    private readonly MasterDbContext _masterContext;

    public LogServices(MasterDbContext masterContext)
    {
        _masterContext = masterContext;
    }

    public async Task<bool> CheckLogExistsAsync(int logId)
    {
        return await _masterContext.SystemLog.AnyAsync(sl => sl.LogId == logId);
    }

    public async Task<int> CountUserLogsAsync(int userId)
    {
        return await _masterContext.SystemLog.CountAsync(sl => sl.AppUserId == userId);
    }

    public async Task<int> DeleteLogAsync(int logId)
    {
        var dbLog = await GetLogByIdWithFindCmdAsync(logId);
        if (dbLog != null)
        {
            _masterContext.SystemLog.Remove(dbLog);
            await _masterContext.SaveChangesAsync();
            return logId;
        }
        else
            throw new KeyNotFoundException("Cannot found log data");
    }

    public async Task<bool> ExecuteDeleteSqlInterpolatedAsync(int logId)
    {
        var result = await _masterContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE SystemLog SET Deleted = 1 WHERE LogId = {logId}");
        return result == 1 ? true : false;
    }

    public async Task<bool> ExecuteDeleteSqlRawAsync(int logId)
    {
        var result = await _masterContext.Database.ExecuteSqlRawAsync($"UPDATE SystemLog SET Deleted = 1 WHERE LogId = {logId}");
        return result == 1 ? true : false;
    }

    public async Task<List<dynamic>> GetAnonymousLogDataList()
    {
        var anonymousList = await _masterContext.SystemLog
        .Select(p => new { p.LogId, p.Description })
        .ToListAsync<dynamic>();
        return anonymousList;
    }

    public async Task<EntityEntry> GetEntryAsync(int logId)
    {
        var dbLog = await GetLogByIdWithFindCmdAsync(logId);
        var logEntry = _masterContext.SystemLog.Entry(dbLog);
        return logEntry;
    }

    //Finds by a key
    public async Task<SystemLog?> GetLogByIdWithFindCmdAsync(int logId)
    {
        return await _masterContext.SystemLog.FindAsync(logId);
    }

    //Finds by condition, if multiple found, first one will return, otherwise if nothing found it returns null
    public async Task<SystemLog?> GetLogByIdWithFirstCmdAsync(int logId)
    {
        return await _masterContext.SystemLog.FirstOrDefaultAsync(sl => sl.LogId == logId);
    }

    //Finds by condition, if multiple found, throws exception, if just one record found, returns that, otherwise it returns null
    public async Task<SystemLog?> GetLogByIdWithSingleCmdAsync(int logId)
    {
        return await _masterContext.SystemLog.SingleOrDefaultAsync(sl => sl.LogId == logId);
    }

    public async Task<List<SystemLog>> GetLogsAsync()
    {
        return await _masterContext.SystemLog.ToListAsync();
    }

    //Implemented on log id because it was the only numeric field so , don't get so strict !
    //Will get average of id values
    public async Task<double> ReadAverageLogIdAsync()
    {
        return await _masterContext.SystemLog.AverageAsync(sl => sl.LogId);
    }

    public async Task<List<dynamic>> ReadGroupedLogsAsync()
    {
        var groupedLogs = await _masterContext.SystemLog
            .GroupBy(p => p.AppUserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToListAsync<dynamic>();
        return groupedLogs;
    }

    public async Task<List<dynamic>> ReadJoinedLogsAsync()
    {
        var userLogs = await _masterContext.SystemLog
            .Join(_masterContext.AppUser,
            sl => sl.AppUserId,
            au => au.AppUserId,
            (sl, au) => new { sl.Description, au.Username, au.EmailAddress })
            .ToListAsync<dynamic>();
        return userLogs;
    }

    //Will get maximum log id from database
    public async Task<int> ReadMaxLogIdAsync()
    {
        return await _masterContext.SystemLog.MaxAsync(sl => sl.LogId);
    }

    //Will get minumum log id from database
    public async Task<int> ReadMinLogIdAsync()
    {
        return await _masterContext.SystemLog.MinAsync(sl => sl.LogId);
    }

    public async Task<int> SaveLogAsync(SystemLog systemLog)
    {
        await _masterContext.AddAsync(systemLog);
        await _masterContext.SaveChangesAsync();

        return systemLog.LogId;
    }

    public async Task<int> UpdateLogAsync(int logId, SystemLog systemLog)
    {
        var dbLog = await GetLogByIdWithFindCmdAsync(logId);
        if (dbLog == null)
            throw new KeyNotFoundException("Log data not found");

        dbLog.Description = systemLog.Description;
        dbLog.LogSerial = systemLog.LogSerial;
        dbLog.LogDateTime = systemLog.LogDateTime;

        _masterContext.SystemLog.Update(dbLog);
        await _masterContext.SaveChangesAsync();

        return logId;
    }
}