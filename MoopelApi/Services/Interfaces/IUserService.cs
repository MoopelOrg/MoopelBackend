using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Update;

namespace MoopelApi.Services.Interfaces;

public interface IUserService
{
    public Task<User?> GetUserById(int id);
    public Task<User?> GetUserByName(string name);

    public Task<List<User>> GetUsers(CancellationToken cancelToken);
    public Task<User?> GetUserFromUserId(int  userId);
    public Task<User?> GetUserFromUserName(string userName);
    public Task<UserSettings?> GetUserSettings(int userId);
    public Task<UserSettings?> GetSettings(int settingsId); 

    public Task<User?> UpdateUser(UserUpdateRequest updateRequest);
    public Task<UserSettings?> UpdateUserSettings(UserSettingsUpdateRequest updateRequest);

    public Task<UserSettings> CreateUserSettings();


    public Task<bool> DisableUser(int userId);

}
