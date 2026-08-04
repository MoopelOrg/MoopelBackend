using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MoopelApi.Models;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Enums;
using MoopelApi.Services.Interfaces;

namespace MoopelApi.Controllers;

[Authorize]
[ApiController]
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
    public async Task<ActionResult<List<Session>>> GetMySessions(CancellationToken cancelToken)
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        User contextUser = await _contextService.GetContextUser(HttpContext);

        _logger.LogInformation("Received request to get sessions for context user from context[{Context}]", context);

        List<Session> sessions = await _sessionService.GetUserSessions(contextUser.UserId, cancelToken);
        return Ok(sessions);
    }

    [HttpGet(Routes.Session.SessionById)]
    public async Task<ActionResult<Session>> GetSessionById(int sessionId)
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        User contextUser = await _contextService.GetContextUser(HttpContext);

        _logger.LogInformation("Received request to get session [{SessionId}] from context[{Context}]", sessionId, context);

        Session? session = await _sessionService.GetSessionById(sessionId);

        if (session is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get session[{SessionId}] which was not found.",
                context, sessionId);
            return NotFound();
        }
        if (session.UserId != contextUser.UserId)
        {
            _logger.LogWarning("Request[{Context}] attempted to get session[{SessionId}] without sufficient permissions.",
                context, sessionId);
            return Forbid();
        }
        return Ok(session);
    }

    [HttpPost(Routes.Session.RevokeSession)]
    public async Task<ActionResult> RevokeSession(int sessionId)
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        User contextUser = await _contextService.GetContextUser(HttpContext);

        _logger.LogInformation("Received request to revoke session [{SessionId}] from context[{Context}]", sessionId, context);

        if (contextUser.Role != UserRole.Admin)
        {
            _logger.LogWarning("Request[{Context}] attempted attempted to revoke session {SessionId} without sufficient permissions",
                context, sessionId);
            return Forbid();
        }

        await _sessionService.RevokeSession(sessionId);

        return Ok();
    }

    [HttpPost(Routes.Session.RevokeAll)]
    public async Task<ActionResult> RevokeAllSessions()
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        User contextUser = await _contextService.GetContextUser(HttpContext);

        _logger.LogInformation("Received request to revoke all sessions from context[{Context}]", context);

        if (contextUser.Role != UserRole.Admin)
        {
            _logger.LogWarning("[{Context}] attempted to revoke all sessions without sufficient permissions", context);
            return Forbid();
        }

        await _sessionService.RevokeAllSessions();
        return Ok();
    }
}