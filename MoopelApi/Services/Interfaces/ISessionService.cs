using MoopelApi.Models.Dto;

namespace MoopelApi.Services.Interfaces;

public interface ISessionService
{
    Task<List<Session>> GetUserSessions(int userId);
    Task<Session> GetSessionById(int sessionId);

    Task<bool> RevokeSession(int sessionId);

    Task<bool> RevokeAllSessions();
}
