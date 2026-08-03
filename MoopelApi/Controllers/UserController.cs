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

        if (context.UserId != userId)
        {
            _logger.LogWarning("User[{ContextUserId}] attempted to get user[{TargetUserId}] from context[{Context}]",
                context.UserId, userId, context);
            return Forbid();
        }

        User? user = await _userService.GetUserFromUserId(userId);

        if (user is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get user[{Id}] which was not found.",
                context, userId);
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet(Routes.User.UserByName)]
    public async Task<ActionResult<UserRead>> GetUserByName(string username)
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to retrieve user[{Username}] from context[{Context}]", username, context);

        User? user = await _userService.GetUserFromUserName(username);

        if (user is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get user[{Name}] which was not found.",
                context, username);
            return NotFound();
        }
        else if (user.UserId != context.UserId)
        {
            _logger.LogWarning("User[{ContextUserId}] attempted to get user[{TargetUserId}] from context[{Context}]",
                context.UserId, user.UserId, context);
            return Forbid();
        }

        return Ok(user);
    }

    [HttpPatch(Routes.User.UpdateUser)]
    public async Task<ActionResult<UserRead>> UpdateUser(int userId, [FromBody] UserUpdateRequest updateRequest)
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        User contextUser = await _contextService.GetContextUser(HttpContext);

        _logger.LogInformation("Received request to update user[{Id}] from context[{Context}]", contextUser.UserId, context);

        if (context.UserId != userId)
        {
            _logger.LogWarning("User[{ContextUserId}] attempted to update user[{TargetUserId}] from context[{Context}]",
                context.UserId, userId, context);
            return Forbid();
        }

        User updatedUser = await _userService.UpdateUser(updateRequest, contextUser.UserId)
            ?? throw new($"Could not find user to update [{contextUser.UserId}]");

        return Ok(updatedUser.ToRead());
    }

    [Authorize(Policy = Policies.AdminOnly)]
    [HttpPatch(Routes.User.DisableUser)]
    public async Task<ActionResult> DisableUser(int userId)
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to disable user[{Id}] from context[{Context}]", userId, context);

        User? user = await _userService.GetUserFromUserId(userId);

        if (user is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to disable user[{Id}] which was not found.",
                context, userId);
            return NotFound();
        }

        await _userService.DisableUser(userId);

        return Ok();
    }
}