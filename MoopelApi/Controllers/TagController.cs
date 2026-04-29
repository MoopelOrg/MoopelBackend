using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoopelApi.Models;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Dto.Read;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;
using MoopelApi.Services.Interfaces;

namespace MoopelApi.Controllers;

[Authorize]
public class TagController : ControllerBase
{
    private readonly ILogger<TagController> _logger;
    private readonly ITagService _tagService;
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly IContextService _contextService;

    public TagController(ILogger<TagController> logger,
                         ITagService tagService,
                         IAuthService authService,
                         IUserService userService,
                         IContextService contextService)
    {
        _logger = logger;
        _tagService = tagService;
        _authService = authService;
        _userService = userService;
        _contextService = contextService;
    }

    #region User Tags
    [HttpGet(Routes.Tag.UserTags)]
    public async Task<ActionResult<List<UserTag>>> GetUserTags()
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        _logger.LogInformation("User {UserId} requested their tags", contextUser.UserId);

        List<UserTag> tags = await _tagService.GetUserTags(contextUser.UserId);
        return Ok(tags);
    }

    [HttpGet(Routes.Tag.UserTagById)]
    public async Task<ActionResult<UserTag>> GetUserTagById(int tagId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        _logger.LogInformation("User {UserId} requested tag {TagId}", contextUser.UserId, tagId);

        UserTag? tag = await _tagService.GetUserTagById(tagId);

        if (tag is null)
        {
            return NotFound();
        }
        if (tag.UserId != contextUser.UserId)
        {
            return Forbid();
        }

        return Ok(tag);
    }

    [HttpPost(Routes.Tag.CreateUserTag)]
    public async Task<ActionResult<UserTag>> CreateUserTag([FromBody] TagCreationRequest creationRequest)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        _logger.LogInformation("User {UserId} is creating a new tag", contextUser.UserId);

        UserTag createdTag = await _tagService.CreateUserTag(creationRequest, contextUser.UserId);

        return CreatedAtAction(nameof(GetUserTagById), new { tagId = createdTag.NoteId }, createdTag);
    }

    [HttpPatch(Routes.Tag.UpdateUserTag)]
    public async Task<ActionResult<UserTag>> UpdateUserTag(int tagId, [FromBody] TagUpdateRequest updateRequest)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        _logger.LogInformation("User {UserId} is updating tag {TagId}", contextUser.UserId, tagId);

        UserTag? userTag = await _tagService.GetUserTagById(tagId);
        if (userTag is null)
        {
            return NotFound();
        }
        if (userTag.UserId != contextUser.UserId)
        {
            return Forbid();
        }

        UserTag? updatedTag = await _tagService.UpdateUserTag(tagId, updateRequest, contextUser.UserId);

        if (updatedTag is null)
        {
            return NotFound();
        }

        return Ok(updatedTag);
    }

    [HttpDelete(Routes.Tag.DeleteUserTag)]
    public async Task<ActionResult> DeleteUserTag(int tagId) 
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        _logger.LogInformation("User {UserId} is deleting tag {TagId}", contextUser.UserId, tagId);

        UserTag? userTag = await _tagService.GetUserTagById(tagId);

        if (userTag is null)
        {
            return NotFound();
        }
        if (userTag.UserId != contextUser.UserId)
        {
            return Forbid();
        }

        bool deleted = await _tagService.DeleteUserTag(tagId, contextUser.UserId);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
    #endregion

    #region Group Tags
    [HttpGet(Routes.Tag.GroupTagsByGroupId)]
    public async Task<ActionResult<List<GroupTag>>> GetGroupTagsByGroupId(int groupId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        _logger.LogInformation("User {UserId} requested tags for group {GroupId}", contextUser.UserId, groupId);

        List<GroupTag> tags = await _tagService.GetGroupTagsByGroupId(groupId);
        return Ok(tags);
    }

    [HttpGet(Routes.Tag.GroupTagById)]
    public async Task<ActionResult<GroupTag>> GetGroupTagById(int tagId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        _logger.LogInformation("User {UserId} requested group tag {TagId}", contextUser.UserId, tagId);
        GroupTag? tag = await _tagService.GetGroupTagById(tagId);
        if (tag is null)
        {
            return NotFound();
        }
        return Ok(tag);
    }

    [HttpPost(Routes.Tag.CreateGroupTag)]
    public async Task<ActionResult<GroupTag>> CreateGroupTag(int groupId, [FromBody] TagCreationRequest creationRequest)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        _logger.LogInformation("User {UserId} is creating a new tag for group {GroupId}", contextUser.UserId, groupId);

        GroupTag createdTag = await _tagService.CreateGroupTag(creationRequest, groupId, contextUser.UserId);
        return CreatedAtAction(nameof(GetGroupTagById), new { tagId = createdTag.TagId }, createdTag);
    }

    [HttpPatch(Routes.Tag.UpdateGroupTag)]
    public async Task<ActionResult<GroupTag>> UpdateGroupTag(int tagId, [FromBody] TagUpdateRequest updateRequest)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        _logger.LogInformation("User {UserId} is updating group tag {TagId}", contextUser.UserId, tagId);

        GroupTag? groupTag = await _tagService.GetGroupTagById(tagId);
        if (groupTag is null)
        {
            return NotFound();
        }
        GroupTag? updatedTag = await _tagService.UpdateGroupTag(tagId, updateRequest, contextUser.UserId);
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
        _logger.LogInformation("User {UserId} is deleting group tag {TagId}", contextUser.UserId, tagId);

        GroupTag? groupTag = await _tagService.GetGroupTagById(tagId);
        if (groupTag is null)
        {
            return NotFound();
        }

        bool deleted = await _tagService.DeleteGroupTag(tagId, contextUser.UserId);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    #endregion
}
