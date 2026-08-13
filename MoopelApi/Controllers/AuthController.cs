using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MoopelApi.Helpers;
using MoopelApi.Models;
using MoopelApi.Services.Interfaces;

using MoopelObjects.Dto;
using MoopelObjects.Dto.Read;
using MoopelObjects.Requests;
using MoopelObjects.Results;

using static MoopelApi.ConstantValues;

namespace MoopelApi.Controllers;

[ApiController]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly IContextService _contextService;
    private readonly IUserService _userService;
    private readonly IAuthService _authService;

    public AuthController(ILogger<AuthController> logger, IContextService contextService,
                          IAuthService authService, IUserService userService)
    {
        _logger = logger;
        _contextService = contextService;
        _authService = authService;
        _userService = userService;
    }

    [HttpPost(Routes.Auth.Login)]
    public async Task<ActionResult<LoginResult>> Login([FromBody] LoginRequest request, CancellationToken cancelToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received Login request for {User} with {Context}",
            request.Username, context);

        LoginResult? loginResult = await _authService.Login(request, context, cancelToken);
        if (loginResult is not null && string.IsNullOrEmpty(loginResult.Error))
        {
            return Ok(loginResult);
        }
        else if (string.IsNullOrEmpty(loginResult?.Error) is false)
        {
            return Unauthorized(loginResult);
        }
        else
        {
            return Unauthorized();
        }
    }

    [HttpPost(Routes.Auth.GuestLogin)]
    public async Task<ActionResult<LoginResult>> GuestLogin(CancellationToken cancelToken)
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received Guest Login request with {Context}", context);

        LoginResult loginResult = await _authService.GuestLogin(context, cancelToken)
            ?? throw new("Failed to complete guest login");

        return Ok(loginResult);
    }

    [HttpPost(Routes.Auth.Register)]
    public async Task<ActionResult<UserRead>> Register([FromBody] RegistrationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received Registration request for {User} with {Context}",
            request.Username, context);

        bool userNameValid = RegexHelper.CheckWithRegex(request.Username, ValidationRegex.UsernameValidationPattern);
        if (!userNameValid)
        {
            _logger.LogInformation("Registration request for {User} with {Context} failed due to username not meeting Regex",
                request.Username, context);
            // TODO: Improve explanation
            return BadRequest(ValidationRegex.UsernameValidationExplanation);
        }

        bool passwordValid = RegexHelper.CheckWithRegex(request.Password, ValidationRegex.PasswordValidationPattern);
        if (!passwordValid)
        {
            _logger.LogInformation("Registration request for {User} with {Context} failed due to password not meeting Regex",
                request.Username, context);
            // TODO: Improve explanation
            return BadRequest(ValidationRegex.PasswordValidationExplanation);
        }

        User? existingUser = await _userService.GetUserFromUserName(request.Username);
        if (existingUser is not null)
        {
            _logger.LogWarning("{Context} attempted to register {Username}, which already existed.",
                context, request.Username);
            return BadRequest($"User '{request.Username}' already exists.");
        }

        User? newUser = await _authService.Register(request);

        if (newUser is not null)
        {
            return Ok(newUser.ToRead());
        }
        else
        {
            return StatusCode(500, "Failed to register user '{}'");
        }
    }

    [Authorize]
    [HttpPost(Routes.Auth.Logout)]
    public async Task<ActionResult<bool>> Logout()
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);

        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received logout request for {Context}", context);

        bool loggedOut = await _authService.LogoutUser(contextUser.UserId);

        _logger.LogInformation("Logout request for {Context} resulted in {Result}", context, loggedOut);
        return Ok(loggedOut);
    }

    [Authorize]
    [HttpGet(Routes.Auth.Me)]
    public async Task<ActionResult<UserRead>> Me()
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received me request for {Context}", context);

        return Ok(contextUser.ToRead());
    }
}
