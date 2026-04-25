using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests;

namespace MoopelApi.Services.Interfaces;

public interface IUserService
{
    public Task<List<User>> GetUsers(CancellationToken cancelToken);
    public Task<User?> GetUserFromUserId(int  userId);
    public Task<User?> GetUserFromUserName(string userName);

    public Task<User?> UpdateUser(UserUpdateRequest updateRequest);

    public Task<bool> DisableUser(int userId);

}
