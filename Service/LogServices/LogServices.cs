using Core;
using DataLayer;

namespace Service.LogServices;

public interface ILogServices
{
    Task<int> SaveLogAsync(SystemLog systemLog);
    Task<int> UpdateLogAsync(int logId, SystemLog systemLog);
    Task<SystemLog> GetLogByIdAsync(int logId);
    Task<List<SystemLog>> GetLogsAsync();
}

public class LogServices : ILogServices
{
    private readonly MasterDbContext _masterContext;

    public LogServices(MasterDbContext masterContext)
    {
        _masterContext = masterContext;
    }

    public Task<SystemLog> GetLogByIdAsync(int logId)
    {
        throw new NotImplementedException();
    }

    public Task<List<SystemLog>> GetLogsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<int> SaveLogAsync(SystemLog systemLog)
    {
        await _masterContext.AddAsync(systemLog);
        await _masterContext.SaveChangesAsync();
        return systemLog.LogId;
    }

    public Task<int> UpdateLogAsync(int logId, SystemLog systemLog)
    {
        throw new NotImplementedException();
    }
}