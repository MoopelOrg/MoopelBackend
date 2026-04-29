using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoopelApi.Models;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Dto.Read;
using MoopelApi.Models.Enums;
using MoopelApi.Models.Requests;
using MoopelApi.Services.Interfaces;

namespace MoopelApi.Controllers;

[Authorize]
public class SessionsController : ControllerBase
{
    private readonly ILogger<SessionsController> _logger;
    private readonly ISessionService _sessionService;
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly IContextService _contextService;
    public SessionsController(ILogger<SessionsController> logger,
                              ISessionService sessionService,
                              IAuthService authService,
                              IUserService userService,
                              IContextService contextService)
    {
        _logger = logger;
        _sessionService = sessionService;
        _authService = authService;
        _userService = userService;
        _contextService = contextService;
    }

    [HttpGet(Routes.Session.MySessions)]
    public async Task<ActionResult<List<Session>>> GetMySessions()
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        _logger.LogInformation("User {UserId} requested their sessions", contextUser.UserId);

        List<Session> sessions = await _sessionService.GetUserSessions(contextUser.UserId);
        return Ok(sessions);
    }

    [HttpGet(Routes.Session.SessionById)]
    public async Task<ActionResult<Session>> GetSessionById(int sessionId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        _logger.LogInformation("User {UserId} requested session {SessionId}", contextUser.UserId, sessionId);
        Session? session = await _sessionService.GetSessionById(sessionId);
        if (session is null)
        {
            return NotFound();
        }
        if (session.UserId != contextUser.UserId)
        {
            return Forbid();
        }
        return Ok(session);
    }

    [HttpPost(Routes.Session.RevokeSession)]
    public async Task<ActionResult> RevokeSession(int sessionId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);

        if (contextUser.Role != UserRole.Admin)
        {
            _logger.LogWarning("User {UserId} attempted to revoke session {SessionId} without sufficient permissions", contextUser.UserId, sessionId);
            return Forbid();
        }

        bool revoked = await _sessionService.RevokeSession(sessionId);

        if (!revoked) 
        {
            _logger.LogWarning("Session {SessionId} could not be revoked by user {UserId}", sessionId, contextUser.UserId);
            return NotFound();
        }

        return Ok();
    }

    [HttpPost(Routes.Session.RevokeAll)]
    public async Task<ActionResult> RevokeAllSessions()
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);

        if (contextUser.Role != UserRole.Admin)
        {
            _logger.LogWarning("User {UserId} attempted to revoke session {SessionId} without sufficient permissions", contextUser.UserId, sessionId);
            return Forbid();
        }

        _logger.LogInformation("User {UserId} requested to revoke all their sessions", contextUser.UserId);
        await _sessionService.RevokeAllSessions(contextUser.UserId);
        return Ok();
    }
}