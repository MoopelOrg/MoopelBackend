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
public class ThemesController : ControllerBase
{
    private readonly ILogger<ThemesController> _logger;
    private readonly IContextService _contextService;
    private readonly IThemeService _themeService;
    private readonly IAuthService _authService;

    public ThemesController(ILogger<ThemesController> logger,
        IContextService contextService,
        IAuthService authService,
        IThemeService themeService)
    {
        _logger = logger;
        _contextService = contextService;
        _themeService = themeService;
        _authService = authService;
    }

    [HttpGet(Routes.Theme.AllThemes)]
    public async Task<ActionResult<List<Theme>>> GetAllThemes(CancellationToken cancelToken)
    {
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to get all themes from requestContext[{Context}]",
            requestContext);

        List<Theme> themes = await _themeService.GetAllThemes(cancelToken);

        return Ok(themes);
    }

    [HttpGet(Routes.Theme.ThemeById)]
    public async Task<ActionResult<Theme>> GetThemeById(int themeId)
    {
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to get theme[{Id}] from requestContext[{Context}]",
            themeId, requestContext);

        Theme? theme = await _themeService.GetThemeById(themeId);

        if (theme is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get theme[{Id}] which was not found.",
                requestContext, themeId);
            return NotFound();
        }

        return Ok(theme);
    }

    [HttpPost(Routes.Theme.CreateTheme)]
    public async Task<ActionResult<Theme>> CreateTheme(ThemeCreateRequest request)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to create theme from requestContext[{Context}]",
            requestContext);

        Theme createdTheme = await _themeService.CreateTheme(request, contextUser);

        return CreatedAtAction(nameof(GetThemeById), new { themeId = createdTheme.ThemeId }, createdTheme);
    }

    [HttpPatch(Routes.Theme.UpdateTheme)]
    public async Task<ActionResult<Theme>> UpdateTheme(int themeId, ThemeUpdateRequest request)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to get theme[{Id}] from requestContext[{Context}]",
            themeId, requestContext);

        Theme? theme = await _themeService.GetThemeById(themeId);

        if (theme is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get theme[{Id}] which was not found.",
                requestContext, themeId);
            return NotFound();
        }

        bool canUpdate = await _authService.CanUserUpdateTheme(contextUser, theme);

        if (canUpdate is false)
        {
            _logger.LogInformation("Request[{Context}] attempted to update theme[{Id}] but was forbidden.",
                requestContext, themeId);
            return Forbid();
        }

        Theme updatedTheme = await _themeService.UpdateTheme(themeId, request);

        return Ok(updatedTheme);
    }
}
