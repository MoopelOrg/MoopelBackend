using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoopelApi.Models;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Dto.Read;
using MoopelApi.Models.Requests.Update;
using MoopelApi.Services.Interfaces;

namespace MoopelApi.Controllers;

[Authorize]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserService _userService;
    private readonly IContextService _contextService;

    public UserController(ILogger<UserController> logger,
                          IUserService userService,
                          IContextService contextService)
    {
        _logger = logger;
        _userService = userService;
        _contextService = contextService;
    }

    [HttpGet(Routes.User.UserById)]
    public async Task<ActionResult<UserRead>> GetUser(int userId)
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to retrieve user[{Id}] from context[{Context}]", userId, context);

        User? user = await _userService.GetUserFromUserId(userId);

        if (user is null)
            return NotFound();

        return Ok(user);
    }

    [HttpGet(Routes.User.UserByName)]
    public async Task<ActionResult<UserRead>> GetUserByName(string username)
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to retrieve user[{Username}] from context[{Context}]", username, context);

        User? user = await _userService.GetUserFromUserName(username);

        if (user is null)
            return NotFound();

        return Ok(user);
    }

    [HttpPatch(Routes.User.UpdateUser)]
    public async Task<ActionResult<UserRead>> UpdateUser([FromBody] UserUpdateRequest updateRequest)
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        User contextUser = await _contextService.GetContextUser(HttpContext);

        _logger.LogInformation("Received request to update user[{Id}] from context[{Context}]", contextUser.UserId, context);

        User? updatedUser = await _userService.UpdateUser(updateRequest);

        if (updatedUser is null)
            return NotFound();
        return Ok(updatedUser.ToRead());
    }

    [HttpPatch(Routes.User.DisableUser)]
    public async Task<ActionResult> DisableUser(int userId)
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        User contextUser = await _contextService.GetContextUser(HttpContext);

        if (contextUser.UserId != userId)
        {
            _logger.LogWarning("User[{ContextUserId}] attempted to disable user[{TargetUserId}] from context[{Context}]", contextUser.UserId, userId, context);
            return Forbid();
        }

        _logger.LogInformation("Received request to disable user[{Id}] from context[{Context}]", userId, context);

        await _userService.DisableUser(userId);

        return NoContent();
    }
}