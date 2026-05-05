using MoopelApi.Models.Configuration;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;
using MoopelApi.Services.Interfaces;

using static MoopelApi.ConstantValues.Database;

namespace MoopelApi.Services;

public class GroupService : IGroupService
{
    private readonly ILogger<GroupService> _logger;
    private readonly INotificationService _notificationService;
    private readonly DatabaseConnection _connection;

    public GroupService(ILogger<GroupService> logger, INotificationService notificationService, DatabaseConnection connection)
    {
        _logger = logger;
        _notificationService = notificationService;
        _connection = connection;
    }

    public Task<Group> CreateGroup(GroupCreateRequest creationRequest)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteGroup(int groupId)
    {
        throw new NotImplementedException();
    }

    public Task<Group> GetGroup(int groupId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Group>> GetUserGroups(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<Group> UpdateGroup(GroupUpdateRequest updateRequest)
    {
        throw new NotImplementedException();
    }
}
