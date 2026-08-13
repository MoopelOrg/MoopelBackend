using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MoopelApi.Models;
using MoopelApi.Services.Interfaces;

using MoopelObjects;
using MoopelObjects.Dto;
using MoopelObjects.Requests.Creation;
using MoopelObjects.Requests.Update;

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

    [HttpGet(ApiRoutes.UserSettings.MySettings)]
    public async Task<ActionResult<UserSettings>> GetMySettings()
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        User contextUser = await _contextService.GetContextUser(HttpContext);

        _logger.LogInformation("Received request to retrieve settings for user[{Id}] from context[{Context}]", contextUser.UserId, context);

        UserSettings? settings = await _userService.GetSettingsFromUserId(contextUser.UserId);

        if (settings is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get settings for user[{Id}] which was not found.",
                context, contextUser.UserId);
            return NotFound();
        }

        return Ok(settings);
    }

    [HttpGet(ApiRoutes.UserSettings.SettingsById)]
    public async Task<ActionResult<UserSettings>> GetSettingsById(int settingsId)
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received request to retrieve settings[{Id}] from context[{Context}]", settingsId, context);

        UserSettings? settings = await _userService.GetSettingsFromSettingId(settingsId);

        if (settings is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get settings[{Id}] which was not found.",
                context, settingsId);
            return NotFound();
        }

        return Ok(settings);
    }

    [HttpPut(ApiRoutes.UserSettings.UpdateSettings)]
    public async Task<ActionResult<UserSettings>> UpdateSettings([FromBody] UserSettingsUpdateRequest updateRequest)
    {
        ArgumentNullException.ThrowIfNull(updateRequest);

        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        User contextUser = await _contextService.GetContextUser(HttpContext);

        _logger.LogInformation("Received request to update settings for user[{Id}] from context[{Context}]", contextUser.UserId, context);

        UserSettings? existingSettings = await _userService.GetSettingsFromUserId(contextUser.UserId);

        if (existingSettings is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to update settings but did not have settings already. Creating new settings.",
                context);
            UserSettingsCreateRequest createRequest = new()
            {
                Timezone = updateRequest.Timezone,
                DateFormat = updateRequest.DateFormat
            };
            existingSettings = await _userService.CreateUserSettings(createRequest, contextUser.UserId);
        }


        UserSettings userSettings = await _userService.UpdateUserSettings(updateRequest, existingSettings.SettingId)
            ?? throw new InvalidOperationException($"Attempted to update user settings [{existingSettings.SettingId}] but it returned null");


        return Ok(userSettings);
    }
}