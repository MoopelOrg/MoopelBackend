using MoopelApi.Models.Dto;

namespace MoopelApi.Services.Interfaces;

public interface ISessionService
{
    Task<List<Session>> GetUserSessions(int userId, CancellationToken cancelToken);
    Task<Session> GetSessionById(int sessionId);

    Task RevokeSession(int sessionId);

    Task<bool> RevokeAllSessions();
}
