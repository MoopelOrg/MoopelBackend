using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;

namespace MoopelApi.Services.Interfaces;

public interface ITagService
{
    Task<List<UserTag>> GetUserTags(int userId);
    Task<UserTag> GetUserTagById(int tagId);
    Task<UserTag> CreateUserTag(TagCreateRequest request, int userId);
    Task<UserTag?> UpdateUserTag(TagUpdateRequest request, int tagId, int userId);
    Task<bool> DeleteUserTag(int tagId, int userId);

    Task<List<GroupTag>> GetGroupTags(int groupId);
    Task<GroupTag> GetGroupTagById(int groupTagId);
    Task<GroupTag> CreateGroupTag(TagCreateRequest request, int groupId);
    Task<GroupTag?> UpdateGroupTag(TagUpdateRequest request, int tagId, int groupId);
    Task<bool> DeleteGroupTag(int tagId, int groupId);
}
