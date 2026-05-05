using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;

namespace MoopelApi.Services.Interfaces;

public interface ITagService
{
    Task<List<UserTag>> GetUserTags(int userId);
    Task<UserTag> GetUserTagById(int tagId);
    Task<UserTag> CreateUserTag(TagCreateRequest request, int userId);
    Task<UserTag?> UpdateUserTag(int tagId, TagUpdateRequest request, int userId);
    Task<bool> DeleteUserTag(int tagId, int userId);

    Task<List<GroupTag>> GetGroupTags(int groupId);
    Task<GroupTag> GetGroupTagById(int groupTagId);
    Task<GroupTag> CreateGroupTag(TagCreateRequest request, int tagId, User creatingUser);
    Task<GroupTag?> UpdateGroupTag(TagUpdateRequest request, int tagId, User updatingUser);
    Task<bool> DeleteGroupTag(int tagId, User deletingUser);
}
