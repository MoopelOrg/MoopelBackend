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
    private readonly IContextService _contextService;
    private readonly IGroupService _groupService;

    public GroupsController(ILogger<GroupsController> logger, IContextService contextService,
        IAuthService authService, IGroupService groupService)
    {
        _logger = logger;
        _authService = authService;
        _contextService = contextService;
        _groupService = groupService;
    }

    [HttpGet(Routes.Group.MyGroups)]
    public async Task<ActionResult<List<Group>>> GetMyGroups()
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        User contextUser = await _contextService.GetContextUser(HttpContext);

        _logger.LogInformation("Received request to retrieve groups for user[{Id}] from context[{Context}]", contextUser.UserId, context);

        List<Group>? groups = await _groupService.GetUserGroups(contextUser.UserId);

        return Ok(groups ?? []);
    }

    [HttpGet(Routes.Group.GroupById)]
    public async Task<ActionResult<Group>> GetGroupById(int groupId)
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received request to retrieve group[{Id}] from context[{Context}]", groupId, context);

        Group? group = await _groupService.GetGroup(groupId);
        if (group is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get group[{GroupId}] which was not found.",
                context, groupId);
            return NotFound();
        }

        return Ok(group);
    }

    [HttpPost(Routes.Group.CreateGroup)]
    public async Task<ActionResult<Group>> CreateGroup([FromBody] GroupCreateRequest request)
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        User contextUser = await _contextService.GetContextUser(HttpContext);

        _logger.LogInformation("Received request[{Request}] to create group from context[{Context}]", request, context);

        List<Group>? groups = await _groupService.GetUserGroups(contextUser.UserId);

        if (groups is not null && groups.Count >= 5)
        {
            _logger.LogWarning("User[{Id}] has reached the maximum number of groups[{Count}]", contextUser.UserId, groups.Count);
            return BadRequest("You have reached the maximum number of groups.");
        }

        Group group = await _groupService.CreateGroup(contextUser, request);

        return Ok(group);
    }

    [HttpPatch(Routes.Group.UpdateGroup)]
    public async Task<ActionResult<Group>> UpdateGroup(int groupId, [FromBody] GroupUpdateRequest request)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received request[{Request}] to update group from context[{Context}]", request, context);

        Group? groupToUpdate = await _groupService.GetGroup(groupId);

        if (groupToUpdate is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to update group[{GroupId}] which was not found.",
                context, groupId);
            return NotFound();
        }

        GroupAccess groupAccess = await _authService.GetUserGroupAccess(contextUser, groupToUpdate);

        if (groupAccess.CanUpdateGroup() is false)
        {
            _logger.LogWarning("Request[{Context}] attempted to update group[{GroupId}] without sufficient permissions.",
                context, groupId);
            return Forbid();
        }

        Group updatedGroup = await _groupService.UpdateGroup(groupId, request)
            ?? throw new Exception("Group update returned null.");

        return Ok(updatedGroup);
    }

    [HttpDelete(Routes.Group.DeleteGroup)]
    public async Task<ActionResult> DeleteGroup(int groupId)
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        User contextUser = await _contextService.GetContextUser(HttpContext);
        _logger.LogInformation("Received request to delete group[{Id}] from context[{Context}]", groupId, context);

        Group? group = await _groupService.GetGroup(groupId);
        if (group is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to delete group[{GroupId}] which was not found.",
                context, groupId);
            return NotFound();
        }

        GroupAccess groupAccess = await _authService.GetUserGroupAccess(contextUser, group);

        if (groupAccess.CanDeleteGroup() is false)
        {
            _logger.LogWarning("Request[{Context}] attempted to delete group[{GroupId}] without sufficient permissions.",
                context, groupId);
            return Forbid();
        }

        await _groupService.DeleteGroup(groupId);

        return NoContent();
    }
}
