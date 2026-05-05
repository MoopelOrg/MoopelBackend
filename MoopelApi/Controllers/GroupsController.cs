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
public class GroupsController : ControllerBase
{
    private readonly ILogger<GroupsController> _logger;
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly IContextService _contextService;
    private readonly IGroupService _groupService;

    public GroupsController(ILogger<GroupsController> logger, IContextService contextService,
        IAuthService authService, IUserService userService, IGroupService groupService)
    {
        _logger = logger;
        _authService = authService;
        _userService = userService;
        _contextService = contextService;
        _groupService = groupService;
    }

    [HttpGet(Routes.Group.MyGroups)]
    public async Task<ActionResult<List<Group>>> GetMyGroups()
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        User contextUser = await _contextService.GetContextUser(HttpContext);

        _logger.LogInformation("Received request to retrieve groups for user[{Id}] from context[{Context}]", contextUser.UserId, context);

        List<Group> groups = await _groupService.GetUserGroups(contextUser.UserId);
        return Ok(groups);
    }

    [HttpGet(Routes.Group.GroupById)]
    public async Task<ActionResult<Group>> GetGroupById(int groupId)
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received request to retrieve group[{Id}] from context[{Context}]", groupId, context);

        Group? group = await _groupService.GetGroup(groupId);
        if (group is null)
            return NotFound();
        return Ok(group);
    }

    [HttpPost(Routes.Group.CreateGroup)]
    public async Task<ActionResult<Group>> CreateGroup([FromBody] GroupCreateRequest request)
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        User contextUser = await _contextService.GetContextUser(HttpContext);

        _logger.LogInformation("Received request[{Request}] to create group from context[{Context}]", request, context);

        List<Group> groups = await _groupService.GetUserGroups(contextUser.UserId);

        if (groups.Count >= 5)
        {
            _logger.LogWarning("User[{Id}] has reached the maximum number of groups[{Count}]", contextUser.UserId, groups.Count);
            return BadRequest("You have reached the maximum number of groups.");
        }

        Group? group = await _groupService.CreateGroup(request);

        if (group is null)
            return BadRequest();

        return Ok(group);
    }

    [HttpPatch(Routes.Group.UpdateGroup)]
    public async Task<ActionResult<Group>> UpdateGroup([FromBody] GroupUpdateRequest request)
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received request[{Request}] to update group from context[{Context}]", request, context);

        Group? group = await _groupService.UpdateGroup(request);

        if (group is null)
            return NotFound();

        return Ok(group);
    }

    [HttpDelete(Routes.Group.DeleteGroup)]
    public async Task<ActionResult> DeleteGroup(int groupId)
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        User contextUser = await _contextService.GetContextUser(HttpContext);
        _logger.LogInformation("Received request to delete group[{Id}] from context[{Context}]", groupId, context);

        Group? group = await _groupService.GetGroup(groupId);
        if (group is null)
            return NotFound();

        GroupAccess groupAccess = await _authService.GetGroupAccess(contextUser, group);

        if (groupAccess.CanDeleteGroup() is false)
            return Forbid();

        bool deleted = await _groupService.DeleteGroup(groupId);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
