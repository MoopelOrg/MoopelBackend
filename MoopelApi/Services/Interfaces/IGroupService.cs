using MoopelApi.Models;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;

namespace MoopelApi.Services.Interfaces;

public interface IGroupService
{
    public Task<List<User>?> GetUsersInGroup(int groupId);
    public Task<List<Group>?> GetUserGroups(int userId);
    public Task<Group?> GetGroup(int groupId);


    public Task<Group> CreateGroup(User creator, GroupCreateRequest creationRequest);

    public Task<Group> UpdateGroup(int groupId, GroupUpdateRequest updateRequest);

    public Task DeleteGroup(int groupId);

    public Task<GroupUser> AddUserToGroup(int groupId, int userId);

    public Task<GroupUser> UpdateGroupUser(int groupId, int userId, GroupUserUpdateRequest updateRequest);
}
