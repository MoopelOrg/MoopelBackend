using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MoopelApi.Exceptions;
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
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        User contextUser = await _contextService.GetContextUser(HttpContext);
        _logger.LogInformation("Received request to get work items for group [{GroupId}] from context[{Context}]",
            groupId, context);

        Group? group = await _groupService.GetGroup(groupId);

        if (group is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get work items for group [{GroupId}] which did not exist.",
                context, groupId);
            return NotFound();
        }
        GroupAccess userAccess = await _authService.GetUserGroupAccess(contextUser, group);

        if (userAccess.CanViewWorkItems is false)
        {
            _logger.LogInformation("Request[{Context}] attempted to get work items for group [{GroupId}] but was forbidden",
                context, groupId);
            return Forbid();
        }

        List<WorkItem>? workItems = await _workItemService.GetWorkItems(groupId, cancelToken);


        return Ok(workItems ?? []);
    }

    [HttpGet(Routes.WorkItem.WorkItemsByGroupIdAndType)]
    public async Task<ActionResult<List<WorkItem>>> GetWorkItemsByType(int groupId, string type, CancellationToken cancelToken)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(type, nameof(type));

        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received request to get work items for group [{GroupId}] with type [{type}] from context[{Context}]",
            groupId, type, context);

        WorkItemType workItemType;
        try
        {
            workItemType = Enum.Parse<WorkItemType>(type, true);
            if (!Enum.IsDefined(workItemType))
            {
                _logger.LogInformation("Request[{Context}] attempted to get work items for [{Type}] which is not valid",
                    context, type);
                return BadRequest($"Invalid work item type: {type}");
            }
        }
        catch
        {
            _logger.LogInformation("Request[{Context}] attempted to get work items for [{Type}] which is not valid",
                context, type);
            return BadRequest($"Invalid work item type: {type}");
        }

        User contextUser = await _contextService.GetContextUser(HttpContext);

        Group? group = await _groupService.GetGroup(groupId);

        if (group is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get work items for group [{GroupId}] which did not exist.",
                context, groupId);
            return NotFound();
        }
        GroupAccess userAccess = await _authService.GetUserGroupAccess(contextUser, group);

        if (userAccess.CanViewWorkItems is false)
        {
            _logger.LogInformation("Request[{Context}] attempted to get work items for group [{GroupId}] but was forbidden.",
                context, groupId);
            return Forbid();
        }

        List<WorkItem>? workItems = await _workItemService.GetWorkItems(groupId, cancelToken, workItemType);

        return Ok(workItems ?? []);
    }

    [HttpGet(Routes.WorkItem.WorkItemById)]
    public async Task<ActionResult<WorkItem>> GetWorkItem(int workItemId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received request to get work item [{ItemId}] from context[{Context}]",
            workItemId, context);


        WorkItem? workItem = await _workItemService.GetWorkItem(workItemId);
        if (workItem is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get work item [{ItemId}] which did not exist.",
                context, workItemId);
            return NotFound();
        }

        Group group = await _groupService.GetGroup(workItem.GroupId)
            ?? throw new NotFoundException($"Group [{workItem.GroupId}] for work item [{workItem.GroupId}] was not found.");

        GroupAccess access = await _authService.GetUserGroupAccess(contextUser, group);
        if (!access.CanViewWorkItems)
        {
            _logger.LogInformation("Request[{Context}] attempted to get work item [{ItemId}] but was forbidden.",
                context, workItemId);
            return Forbid();
        }

        return Ok(workItem);
    }

    [HttpPost(Routes.WorkItem.CreateWorkItem)]
    public async Task<ActionResult<WorkItem>> CreateWorkItem(int groupId, [FromBody] WorkItemCreateRequest request)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received request to create work item for group [{GroupId}] from context[{Context}]",
            groupId, context);

        Group? group = await _groupService.GetGroup(groupId);
        if (group is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to create work item for group [{GroupId}] which did not exist.",
                context, groupId);
            return NotFound();
        }

        GroupAccess access = await _authService.GetUserGroupAccess(contextUser, group);
        if (access.CanCreateWorkItem() is false)
        {
            _logger.LogInformation("Request[{Context}] attempted to create work item for group [{GroupId}] but was forbidden.",
                context, groupId);
            return Forbid();
        }

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
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received request to update work item [{ItemId}] from context[{Context}]",
            workItemId, context);


        WorkItem? existingWorkItem = await _workItemService.GetWorkItem(workItemId);
        if (existingWorkItem is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to update work item [{ItemId}] which did not exist.",
                context, workItemId);
            return NotFound();
        }

        Group group = await _groupService.GetGroup(existingWorkItem.GroupId)
            ?? throw new NotFoundException($"Group [{existingWorkItem.GroupId}] for work item [{workItemId}] was not found.");

        GroupAccess access = await _authService.GetUserGroupAccess(contextUser, group);
        if (access.CanUpdateWorkItem() is false)
        {
            _logger.LogInformation("Request[{Context}] attempted to update work item [{ItemId}] but was forbidden.",
                context, workItemId);
            return Forbid();
        }

        WorkItem updatedWorkItem = await _workItemService.UpdateWorkItem(request, workItemId);

        return Ok(updatedWorkItem);
    }

    [HttpDelete(Routes.WorkItem.DeleteWorkItem)]
    public async Task<ActionResult> DeleteWorkItem(int workItemId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received request to delete work item [{ItemId}] from context[{Context}]",
            workItemId, context);

        WorkItem? workItem = await _workItemService.GetWorkItem(workItemId);
        if (workItem is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to delete work item [{ItemId}] which did not exist.",
                context, workItemId);
            return NotFound();
        }

        Group group = await _groupService.GetGroup(workItem.GroupId)
            ?? throw new NotFoundException($"Group [{workItem.GroupId}] for work item [{workItemId}] was not found.");

        GroupAccess access = await _authService.GetUserGroupAccess(contextUser, group);
        if (!access.CanDeleteWorkItems)
        {
            _logger.LogInformation("Request[{Context}] attempted to delete work item [{ItemId}] but was forbidden.",
                context, workItemId);
            return Forbid();
        }

        await _workItemService.DeleteWorkItem(workItemId);

        return NoContent();
    }

    [HttpPost(Routes.WorkItem.AddLink)]
    public async Task<ActionResult> AddLink([FromBody] WorkItemLinkRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        string logBetween = $"ParentWorkItemId[{request.ParentWorkItemId}] and ChildWorkItemId[{request.ChildWorkItemId}]";

        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received request to add work item link [{Between}] from context[{Context}]",
            logBetween, context);

        WorkItem? child = await _workItemService.GetWorkItem(request.ChildWorkItemId);
        WorkItem? parent = await _workItemService.GetWorkItem(request.ParentWorkItemId);

        if (child is null || parent is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to add work item link [{Between}], one was not found.",
                context, logBetween);
            return NotFound();
        }

        if (child.GroupId != parent.GroupId)
        {
            _logger.LogInformation("Request[{Context}] attempted to add work item link [{Between}], which are not in the same group.",
                context, logBetween);
            return BadRequest("Parent and child work items must belong to the same group.");
        }

        if (child.ItemId == parent.ItemId)
        {
            _logger.LogInformation("Request[{Context}] attempted to add work item link [{Between}], which are the same item.",
                context, logBetween);
            return BadRequest("A work item cannot be its own parent.");
        }

        Group? group = await _groupService.GetGroup(child.GroupId)
            ?? throw new NotFoundException($"Group [{child.GroupId}] for work item [{child.ItemId}] was not found.");

        GroupAccess access = await _authService.GetUserGroupAccess(contextUser, group);
        if (!access.CanUpdateWorkItems)
        {
            _logger.LogInformation("Request[{Context}] attempted to add work item link [{Between}], but was forbidden.",
                context, logBetween);
            return Forbid();
        }

        await _workItemService.AddWorkItemLink(parent.ItemId, child.ItemId);

        return NoContent();
    }

    [HttpPost(Routes.WorkItem.RemoveLink)]
    public async Task<IActionResult> RemoveLink([FromBody] WorkItemLinkRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        string logBetween = $"ParentWorkItemId[{request.ParentWorkItemId}] and ChildWorkItemId[{request.ChildWorkItemId}]";

        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received request to remove work item link [{Between}] from context[{Context}]",
            logBetween, context);

        WorkItem? child = await _workItemService.GetWorkItem(request.ChildWorkItemId);
        WorkItem? parent = await _workItemService.GetWorkItem(request.ParentWorkItemId);

        if (child is null || parent is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to remove work item link [{Between}], one was not found.",
                context, logBetween);
            return NotFound();
        }

        if (child.GroupId != parent.GroupId)
        {
            _logger.LogInformation("Request[{Context}] attempted to remove work item link [{Between}], which are not in the same group.",
                context, logBetween);
            return BadRequest("Parent and child work items must belong to the same group.");
        }

        Group group = await _groupService.GetGroup(child.GroupId)
            ?? throw new NotFoundException($"Group [{child.GroupId}] for work item [{child.ItemId}] was not found.");

        GroupAccess access = await _authService.GetUserGroupAccess(contextUser, group);
        if (!access.CanCreateWorkItems)
        {
            _logger.LogInformation("Request[{Context}] attempted to remove work item link [{Between}], but was forbidden.",
                context, logBetween);
            return Forbid();
        }

        await _workItemService.RemoveWorkItemLink(request.ParentWorkItemId, request.ChildWorkItemId);

        return NoContent();
    }
}