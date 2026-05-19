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
public class TagController : ControllerBase
{
    private readonly ILogger<TagController> _logger;
    private readonly ITagService _tagService;
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly IGroupService _groupService;
    private readonly IContextService _contextService;

    public TagController(ILogger<TagController> logger,
                         ITagService tagService,
                         IAuthService authService,
                         IUserService userService,
                         IGroupService groupService,
                         IContextService contextService)
    {
        _logger = logger;
        _tagService = tagService;
        _authService = authService;
        _userService = userService;
        _groupService = groupService;
        _contextService = contextService;
    }

    #region User Tags
    [HttpGet(Routes.Tag.UserTags)]
    public async Task<ActionResult<List<UserTag>>> GetUserTags()
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received request to get users own tags from requestContext[{Context}]", context);

        List<UserTag> tags = await _tagService.GetUserTags(contextUser.UserId);
        return Ok(tags);
    }

    [HttpGet(Routes.Tag.UserTagById)]
    public async Task<ActionResult<UserTag>> GetUserTagById(int tagId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received request to get user tag {TagId} from requestContext[{Context}]", tagId, context);

        UserTag? tag = await _tagService.GetUserTagById(tagId);

        if (tag is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get userTag[{Id}] which was not found.", context, tagId);
            return NotFound();
        }
        if (tag.UserId != contextUser.UserId)
        {
            _logger.LogWarning("Request[{Context}] attempted to get userTag[{Id}] which they do not own.", context, tagId);
            return Forbid();
        }

        return Ok(tag);
    }

    [HttpPost(Routes.Tag.CreateUserTag)]
    public async Task<ActionResult<UserTag>> CreateUserTag([FromBody] TagCreateRequest creationRequest)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received request to create user tag from requestContext[{Context}]", context);

        UserTag createdTag = await _tagService.CreateUserTag(creationRequest, contextUser.UserId);

        return CreatedAtAction(nameof(GetUserTagById), new { tagId = createdTag.TagId }, createdTag);
    }

    [HttpPatch(Routes.Tag.UpdateUserTag)]
    public async Task<ActionResult<UserTag>> UpdateUserTag(int tagId, [FromBody] TagUpdateRequest updateRequest)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received request to update user tag[{Id}] from requestContext[{Context}]", tagId, context);

        UserTag? userTag = await _tagService.GetUserTagById(tagId);
        if (userTag is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get userTag[{Id}] which was not found.", context, tagId);
            return NotFound();
        }
        if (userTag.UserId != contextUser.UserId)
        {
            _logger.LogWarning("Request[{Context}] attempted to update userTag[{Id}] which they do not own.", context, tagId);
            return Forbid();
        }

        UserTag updatedTag = await _tagService.UpdateUserTag(updateRequest, tagId)
            ?? throw new($"Attempted to update usertag [{tagId}] but it returned null");

        return Ok(updatedTag);
    }

    [HttpDelete(Routes.Tag.DeleteUserTag)]
    public async Task<ActionResult> DeleteUserTag(int tagId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received request to update user tag[{Id}] from requestContext[{Context}]", tagId, context);

        UserTag? userTag = await _tagService.GetUserTagById(tagId);

        if (userTag is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get userTag[{Id}] which was not found.", context, tagId);
            return NotFound();
        }
        if (userTag.UserId != contextUser.UserId)
        {
            _logger.LogWarning("Request[{Context}] attempted to delete userTag[{Id}] which they do not own.", context, tagId);
            return Forbid();
        }

        await _tagService.DeleteUserTag(tagId, contextUser.UserId);

        return NoContent();
    }
    #endregion

    #region Group Tags
    [HttpGet(Routes.Tag.GroupTagsByGroupId)]
    public async Task<ActionResult<List<GroupTag>>> GetGroupTagsByGroupId(int groupId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received request to update group tags for Group[{Id}] from requestContext[{Context}]",
            groupId, context);

        Group? group = await _groupService.GetGroup(groupId);

        if (group is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get group[{Id}] which was not found.",
                context, groupId);
            return NotFound();
        }

        GroupAccess access = await _authService.GetUserGroupAccess(contextUser, group);

        if (access.CanViewGroupDetails() is false)
        {
            _logger.LogWarning("Request[{Context}] attempted to get group tags for group[{Id}] which they do not have access to.",
                context, groupId);
            return Forbid();
        }

        List<GroupTag> tags = await _tagService.GetGroupTags(groupId);
        return Ok(tags);
    }

    [HttpGet(Routes.Tag.GroupTagById)]
    public async Task<ActionResult<GroupTag>> GetGroupTagById(int tagId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received request to update group tag[{Id}] from requestContext[{Context}]", tagId, context);

        GroupTag? tag = await _tagService.GetGroupTagById(tagId);
        if (tag is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get groupTag[{Id}] which was not found.",
                context, tagId);
            return NotFound();
        }

        Group group = await _groupService.GetGroup(tag.GroupId)
            ?? throw new($"Could not find group for group tag [{tag}]");

        GroupAccess access = await _authService.GetUserGroupAccess(contextUser, group);

        if (access.CanViewGroupDetails() is false)
        {
            _logger.LogWarning("Request[{Context}] attempted to get group tags for group[{Id}] which they do not have access to.",
                context, group.GroupId);
            return Forbid();
        }

        return Ok(tag);
    }

    [HttpPost(Routes.Tag.CreateGroupTag)]
    public async Task<ActionResult<GroupTag>> CreateGroupTag(int groupId, [FromBody] TagCreateRequest creationRequest)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received request to create group tag from requestContext[{Context}]", context);

        Group? group = await _groupService.GetGroup(groupId);

        if (group is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get group[{Id}] which was not found.",
                context, groupId);
            return NotFound();
        }

        GroupAccess access = await _authService.GetUserGroupAccess(contextUser, group);

        if (access.CanCreateTag() is false)
        {
            _logger.LogWarning("Request[{Context}] attempted to create group tag for group[{Id}] which they do not have access to.",
                context, group.GroupId);
            return Forbid();
        }

        GroupTag createdTag = await _tagService.CreateGroupTag(creationRequest, groupId);
        return CreatedAtAction(nameof(GetGroupTagById), new { tagId = createdTag.TagId }, createdTag);
    }

    [HttpPatch(Routes.Tag.UpdateGroupTag)]
    public async Task<ActionResult<GroupTag>> UpdateGroupTag(int tagId, [FromBody] TagUpdateRequest updateRequest)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received request to update group tag[{Id}] from requestContext[{Context}]", tagId, context);

        GroupTag? groupTag = await _tagService.GetGroupTagById(tagId);
        if (groupTag is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get groupTag[{Id}] which was not found.",
                context, tagId);
            return NotFound();
        }

        Group group = await _groupService.GetGroup(groupTag.GroupId)
            ?? throw new($"Could not find group for group tag [{groupTag}]");

        GroupAccess access = await _authService.GetUserGroupAccess(contextUser, group);

        if (access.CanUpdateTag() is false)
        {
            _logger.LogWarning("Request[{Context}] attempted to get group tags for group[{Id}] which they do not have access to.",
                context, group.GroupId);
            return Forbid();
        }

        GroupTag? updatedTag = await _tagService.UpdateGroupTag(updateRequest, tagId);
        if (updatedTag is null)
        {
            return NotFound();
        }

        return Ok(updatedTag);
    }

    [HttpDelete(Routes.Tag.DeleteGroupTag)]
    public async Task<ActionResult> DeleteGroupTag(int tagId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        _logger.LogInformation("Received request to delete group tag[{Id}] from requestContext[{Context}]", tagId, context);

        GroupTag? groupTag = await _tagService.GetGroupTagById(tagId);
        if (groupTag is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get groupTag[{Id}] which was not found.",
                context, tagId);
            return NotFound();
        }

        Group group = await _groupService.GetGroup(groupTag.GroupId)
            ?? throw new($"Could not find group for group tag [{groupTag}]");

        GroupAccess access = await _authService.GetUserGroupAccess(contextUser, group);

        if (access.CanDeleteTag() is false)
        {
            _logger.LogWarning("Request[{Context}] attempted to get group tags for group[{Id}] which they do not have access to.",
                context, group.GroupId);
            return Forbid();
        }

        await _tagService.DeleteGroupTag(tagId);

        return NoContent();
    }

    #endregion
}
