using MoopelApi.Models;

using MoopelObjects.Dto;
using MoopelObjects.Dto.Read;

namespace MoopelApi.Services.Interfaces;

public interface ISessionService
{
    Task<Session> GenerateSession(RequestContext context, UserRead user, Login? login, string token);

    Task<List<Session>> GetUserSessions(int userId, CancellationToken cancelToken);

    Task<List<Session>> GetGuestUserSessions(CancellationToken cancelToken);

    Task<Session?> GetSessionById(int sessionId);

    Task RevokeSession(int sessionId);

    Task<bool> RevokeAllSessions();
}
