using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoopelApi.Models;
using MoopelApi.Models.Dto.Read;
using MoopelApi.Models.Requests;
using MoopelApi.Services.Interfaces;

namespace MoopelApi.Controllers;

[ApiController]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly IContextService _contextService;

    public AuthController(ILogger<AuthController> logger, IContextService contextService)
    {
        _logger = logger;
        _contextService = contextService;
    }

    [HttpPost(Routes.Auth.Login)]
    public async Task Login([FromBody] LoginRequest request)
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received Login request for {User} with {Context}",
            request.Username, context);
    }

    [HttpPost(Routes.Auth.Register)]
    public async Task<ActionResult<UserRead>> Register([FromBody] RegistrationRequest request)
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received Registration request for {User} with {Context}",
            request.Username, context);

        return BadRequest();
    }

    [Authorize]
    [HttpPost(Routes.Auth.Logout)]
    public async Task Logout()
    {
        _logger.LogInformation("Logout");
    }

    [Authorize]
    [HttpGet(Routes.Auth.Me)]
    public async Task Me()
    {
        _logger.LogInformation("Me");
    }
}
