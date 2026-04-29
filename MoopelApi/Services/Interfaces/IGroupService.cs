using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;

namespace MoopelApi.Services.Interfaces;

public interface IGroupService
{
    public Task<List<Group>> GetUserGroups(int userId);
    public Task<Group> GetGroup(int groupId);


    public Task<Group> CreateGroup(GroupCreateRequest creationRequest);

    public Task<Group> UpdateGroup(GroupUpdateRequest updateRequest);

    public Task<bool> DeleteGroup(int groupId);
}
