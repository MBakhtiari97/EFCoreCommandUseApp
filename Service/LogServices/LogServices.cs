using Core;
using DataLayer;
using Microsoft.EntityFrameworkCore;

namespace Service.LogServices;

public interface ILogServices
{
    Task<int> SaveLogAsync(SystemLog systemLog);
    Task<int> UpdateLogAsync(int logId, SystemLog systemLog);
    Task<int> DeleteLogAsync(int logId);
    Task<SystemLog?> GetLogByIdAsync(int logId);
    Task<List<SystemLog>> GetLogsAsync();
}

public class LogServices : ILogServices
{
    private readonly MasterDbContext _masterContext;

    public LogServices(MasterDbContext masterContext)
    {
        _masterContext = masterContext;
    }

    public async Task<int> DeleteLogAsync(int logId)
    {
        var dbLog = await GetLogByIdAsync(logId);
        if (dbLog != null)
        {
            _masterContext.SystemLog.Remove(dbLog);
            await _masterContext.SaveChangesAsync();
            return logId;
        }
        else
            throw new KeyNotFoundException("Cannot found log data");
    }

    public async Task<SystemLog?> GetLogByIdAsync(int logId)
    {
        return await _masterContext.SystemLog.FindAsync(logId);
    }

    public async Task<List<SystemLog>> GetLogsAsync()
    {
        return await _masterContext.SystemLog.ToListAsync();
    }

    public async Task<int> SaveLogAsync(SystemLog systemLog)
    {
        await _masterContext.AddAsync(systemLog);
        await _masterContext.SaveChangesAsync();

        return systemLog.LogId;
    }

    public async Task<int> UpdateLogAsync(int logId, SystemLog systemLog)
    {
        var dbLog = await GetLogByIdAsync(logId);
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