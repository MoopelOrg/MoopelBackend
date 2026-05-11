using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoopelApi.Models;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Enums;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;
using MoopelApi.Services.Interfaces;

namespace MoopelApi.Controllers;

[Authorize]
[ApiController]
public class WorkItemsController : ControllerBase
{
    private readonly ILogger<WorkItemsController> _logger;
    private readonly IWorkItemService _workItemService;
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly IGroupService _groupService;
    private readonly IContextService _contextService;

    public WorkItemsController(ILogger<WorkItemsController> logger,
                               IWorkItemService workItemService,
                               IAuthService authService,
                               IUserService userService,
                               IGroupService groupService,
                               IContextService contextService)
    {
        _logger = logger;
        _workItemService = workItemService;
        _authService = authService;
        _userService = userService;
        _contextService = contextService;
        _groupService = groupService;
    }

    [HttpGet(Routes.WorkItem.WorkItemsByGroupId)]
    public async Task<ActionResult<List<WorkItem>>> GetWorkItems(int groupId, CancellationToken cancelToken)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        _logger.LogInformation("User {UserId} requested their work items for group {GroupId}", contextUser.UserId, groupId);

        Group? group = await _groupService.GetGroup(groupId);

        if (group is null)
        {
            return NotFound();
        }
        GroupAccess userAccess = await _authService.GetUserGroupAccess(contextUser, group);

        if (userAccess.CanViewWorkItems is false)
        {
            return Forbid();
        }

        List<WorkItem>? workItems = await _workItemService.GetWorkItems(groupId, cancelToken);


        return Ok(workItems);
    }

    [HttpGet(Routes.WorkItem.WorkItemsByGroupIdAndType)]
    public async Task<ActionResult<List<WorkItem>>> GetWorkItemsByType(int groupId, string type, CancellationToken cancelToken)
    {
        WorkItemType workItemType;
        try
        {
            workItemType = Enum.Parse<WorkItemType>(type, true);
            if (!Enum.IsDefined(workItemType))
            {
                return BadRequest($"Invalid work item type: {type}");
            }
        }
        catch
        {
            return BadRequest($"Invalid work item type: {type}");
        }

        User contextUser = await _contextService.GetContextUser(HttpContext);
        _logger.LogInformation("User {UserId} requested their work items for group {GroupId}", contextUser.UserId, groupId);

        Group? group = await _groupService.GetGroup(groupId);

        if (group is null)
        {
            return NotFound();
        }
        GroupAccess userAccess = await _authService.GetUserGroupAccess(contextUser, group);

        if (userAccess.CanViewWorkItems is false)
        {
            return Forbid();
        }

        List<WorkItem>? workItems = await _workItemService.GetWorkItems(groupId, cancelToken, workItemType);
        return Ok(workItems);
    }

    [HttpGet(Routes.WorkItem.WorkItemById)]
    public async Task<ActionResult<WorkItem>> GetWorkItem(int workItemId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);

        WorkItem? workItem = await _workItemService.GetWorkItem(workItemId);
        if (workItem is null)
            return NotFound();

        Group? group = await _groupService.GetGroup(workItem.GroupId);
        if (group is null)
            return NotFound();

        GroupAccess access = await _authService.GetUserGroupAccess(contextUser, group);
        if (!access.CanViewWorkItems)
            return Forbid();

        return Ok(workItem);
    }

    [HttpPost(Routes.WorkItem.CreateWorkItem)]
    public async Task<ActionResult<WorkItem>> CreateWorkItem(int groupId, [FromBody] WorkItemCreateRequest request)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);

        Group? group = await _groupService.GetGroup(groupId);
        if (group is null)
            return NotFound();

        GroupAccess access = await _authService.GetUserGroupAccess(contextUser, group);
        if (!access.CanCreateWorkItems)
            return Forbid();

        WorkItem workItem = await _workItemService.CreateWorkItem(request, group.GroupId);

        return CreatedAtAction(
            nameof(GetWorkItem),
            new { workItemId = workItem.ItemId },
            workItem);
    }

    [HttpPut(Routes.WorkItem.UpdateWorkItem)]
    public async Task<ActionResult<WorkItem>> UpdateWorkItem(int workItemId, [FromBody] WorkItemUpdateRequest request)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);

        WorkItem? existingWorkItem = await _workItemService.GetWorkItem(workItemId);
        if (existingWorkItem is null)
            return NotFound();

        Group? group = await _groupService.GetGroup(existingWorkItem.GroupId);
        if (group is null)
            return NotFound();

        GroupAccess access = await _authService.GetUserGroupAccess(contextUser, group);
        if (!access.CanUpdateWorkItems)
            return Forbid();

        WorkItem updatedWorkItem = await _workItemService.UpdateWorkItem(request, workItemId);

        return Ok(updatedWorkItem);
    }

    [HttpDelete(Routes.WorkItem.DeleteWorkItem)]
    public async Task<ActionResult> DeleteWorkItem(int workItemId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);

        WorkItem? workItem = await _workItemService.GetWorkItem(workItemId);
        if (workItem is null)
            return NotFound();

        Group? group = await _groupService.GetGroup(workItem.GroupId);
        if (group is null)
            return NotFound();

        GroupAccess access = await _authService.GetUserGroupAccess(contextUser, group);
        if (!access.CanDeleteWorkItems)
            return Forbid();

        await _workItemService.DeleteWorkItem(workItemId);

        return NoContent();
    }

    [HttpPost(Routes.WorkItem.AddLink)]
    public async Task<ActionResult> AddLink([FromBody] WorkItemLinkRequest request)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);

        WorkItem? child = await _workItemService.GetWorkItem(request.ChildWorkItemId);
        WorkItem? parent = await _workItemService.GetWorkItem(request.ParentWorkItemId);

        if (child is null || parent is null)
            return NotFound();

        if (child.GroupId != parent.GroupId)
            return BadRequest("Parent and child work items must belong to the same group.");

        if (child.ItemId == parent.ItemId)
            return BadRequest("A work item cannot be its own parent.");

        Group? group = await _groupService.GetGroup(child.GroupId);
        if (group is null)
            return NotFound();

        GroupAccess access = await _authService.GetUserGroupAccess(contextUser, group);
        if (!access.CanUpdateWorkItems)
            return Forbid();

        await _workItemService.AddWorkItemLink(parent.ItemId, child.ItemId);

        return NoContent();
    }

    [HttpPost(Routes.WorkItem.RemoveLink)]
    public async Task<IActionResult> RemoveLink([FromBody] WorkItemLinkRequest request)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);

        WorkItem? child = await _workItemService.GetWorkItem(request.ChildWorkItemId);
        WorkItem? parent = await _workItemService.GetWorkItem(request.ParentWorkItemId);

        if (child is null || parent is null)
            return NotFound();

        if (child.GroupId != parent.GroupId)
            return BadRequest("Parent and child work items must belong to the same group.");

        Group? group = await _groupService.GetGroup(child.GroupId);
        if (group is null)
            return NotFound();

        GroupAccess access = await _authService.GetUserGroupAccess(contextUser, group);
        if (!access.CanCreateWorkItems)
            return Forbid();

        await _workItemService.RemoveWorkItemLink(request.ParentWorkItemId, request.ChildWorkItemId);

        return NoContent();
    }
}