using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;

namespace MoopelApi.Services.Interfaces;

public interface IUserService
{
    public Task<List<User>> GetUsers(CancellationToken cancelToken);
    public Task<User?> GetUserFromUserId(int userId);
    public Task<User?> GetUserFromUserName(string userName);
    public Task<User?> UpdateUser(UserUpdateRequest updateRequest);

    public Task DisableUser(int userId);

    public Task<UserSettings?> GetSettingsFromUserId(int userId);
    public Task<UserSettings?> GetSettingsFromSettingId(int settingsId);
    public Task<UserSettings?> UpdateUserSettings(UserSettingsUpdateRequest updateRequest);

    public Task<UserSettings> CreateUserSettings(int userId, UserSettingsCreateRequest createRequest);
}
