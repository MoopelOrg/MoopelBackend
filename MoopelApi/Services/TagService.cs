using MoopelApi.Models.Configuration;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;
using MoopelApi.Services.Interfaces;

using static MoopelApi.ConstantValues.Database;

namespace MoopelApi.Services;

public class TagService : ITagService
{
    private readonly ILogger<TagService> _logger;
    private readonly INotificationService _notificationService;
    private readonly DatabaseConnection _connection;

    public TagService(ILogger<TagService> logger, INotificationService notificationService, DatabaseConnection connection)
    {
        _logger = logger;
        _notificationService = notificationService;
        _connection = connection;
    }

    public Task<GroupTag> CreateGroupTag(TagCreateRequest request, int tagId, User creatingUser)
    {
        throw new NotImplementedException();
    }

    public Task<UserTag> CreateUserTag(TagCreateRequest request, int userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteGroupTag(int tagId, User deletingUser)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteUserTag(int tagId, int userId)
    {
        throw new NotImplementedException();
    }

    public Task<GroupTag> GetGroupTagById(int groupTagId)
    {
        throw new NotImplementedException();
    }

    public Task<List<GroupTag>> GetGroupTags(int groupId)
    {
        throw new NotImplementedException();
    }

    public Task<UserTag> GetUserTagById(int tagId)
    {
        throw new NotImplementedException();
    }

    public Task<List<UserTag>> GetUserTags(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<GroupTag?> UpdateGroupTag(TagUpdateRequest request, int tagId, User updatingUser)
    {
        throw new NotImplementedException();
    }

    public Task<UserTag?> UpdateUserTag(int tagId, TagUpdateRequest request, int userId)
    {
        throw new NotImplementedException();
    }
}
