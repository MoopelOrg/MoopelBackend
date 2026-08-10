using MoopelApi.Models;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Dto.Read;

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
