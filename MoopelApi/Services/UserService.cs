using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Update;
using MoopelApi.Services.Interfaces;

namespace MoopelApi.Services;

public class UserService : IUserService
{
    public Task<UserSettings> CreateUserSettings()
    {
        throw new NotImplementedException();
    }

    public Task<bool> DisableUser(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<UserSettings?> GetSettings(int settingsId)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetUserById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetUserByName(string name)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetUserFromUserId(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetUserFromUserName(string userName)
    {
        throw new NotImplementedException();
    }

    public Task<List<User>> GetUsers(CancellationToken cancelToken)
    {
        throw new NotImplementedException();
    }

    public Task<UserSettings?> GetUserSettings(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<User?> UpdateUser(UserUpdateRequest updateRequest)
    {
        throw new NotImplementedException();
    }

    public Task<UserSettings?> UpdateUserSettings(UserSettingsUpdateRequest updateRequest)
    {
        throw new NotImplementedException();
    }
}
