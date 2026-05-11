using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoopelApi.Models;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;
using MoopelApi.Services.Interfaces;

namespace MoopelApi.Controllers;

[Authorize]
[ApiController]
public class UserSettingsController : ControllerBase
{
    private readonly ILogger<UserSettingsController> _logger;
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly IContextService _contextService;

    public UserSettingsController(ILogger<UserSettingsController> logger, IContextService contextService,
        IAuthService authService, IUserService userService)
    {
        _logger = logger;
        _authService = authService;
        _userService = userService;
        _contextService = contextService;
    }

    [HttpGet(Routes.UserSettings.MySettings)]
    public async Task<ActionResult<UserSettings>> GetMySettings()
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        User contextUser = await _contextService.GetContextUser(HttpContext);

        _logger.LogInformation("Received request to retrieve settings for user[{Id}] from context[{Context}]", contextUser.UserId, context);

        UserSettings? settings = await _userService.GetSettingsFromUserId(contextUser.UserId);

        if (settings is null)
            return NotFound();

        return Ok(settings);
    }

    [HttpGet(Routes.UserSettings.SettingsById)]
    public async Task<ActionResult<UserSettings>> GetSettingsById(int settingsId)
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received request to retrieve settings[{Id}] from context[{Context}]", settingsId, context);

        UserSettings? settings = await _userService.GetSettingsFromSettingId(settingsId);

        if (settings is null)
            return NotFound();

        return Ok(settings);
    }

    [HttpPatch(Routes.UserSettings.UpdateSettings)]
    public async Task<ActionResult<UserSettings>> UpdateSettings([FromBody] UserSettingsUpdateRequest updateRequest)
    {
        ArgumentNullException.ThrowIfNull(updateRequest);

        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        User contextUser = await _contextService.GetContextUser(HttpContext);

        _logger.LogInformation("Received request to update settings for user[{Id}] from context[{Context}]", contextUser.UserId, context);

        UserSettings? existingSettings = await _userService.GetSettingsFromUserId(contextUser.UserId);
        if (existingSettings is null)
        {
            UserSettingsCreateRequest createRequest = new()
            {
                Timezone = updateRequest.Timezone,
                DateFormat = updateRequest.DateFormat
            };
            await _userService.CreateUserSettings(contextUser.UserId, createRequest);
        }


        UserSettings? userSettings = await _userService.UpdateUserSettings(updateRequest);

        if (userSettings is null)
            return NotFound();

        return Ok(userSettings);
    }
}