using MoopelApi.Models.Configuration;
using MoopelApi.Models.Dto;
using MoopelApi.Services.Interfaces;

using static MoopelApi.ConstantValues.Database;

namespace MoopelApi.Services;

public class SessionService : ISessionService
{
    private readonly ILogger<SessionService> _logger;
    private readonly INotificationService _notificationService;
    private readonly DatabaseConnection _connection;

    public SessionService(ILogger<SessionService> logger, INotificationService notificationService, DatabaseConnection connection)
    {
        _logger = logger;
        _notificationService = notificationService;
        _connection = connection;
    }

    public Task<Session> GetSessionById(int sessionId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Session>> GetUserSessions(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RevokeAllSessions()
    {
        throw new NotImplementedException();
    }

    public Task<bool> RevokeSession(int sessionId)
    {
        throw new NotImplementedException();
    }
}
