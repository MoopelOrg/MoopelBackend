using MoopelApi.Models;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests;
using MoopelApi.Models.Results;

namespace MoopelApi.Services.Interfaces;

public interface IAuthService
{
    public Task<User?> Register(RegistrationRequest request);
    public Task<LoginResult?> Login(LoginRequest request);
    public Task<bool> LogoutUser(int userId);



    public Task<GroupAccess> GetUserGroupAccess(User user, Group group);
    public Task<List<GroupAccess>> GetGroupAccess(Group group);

    public Task<bool> CanUserUpdateTheme(User user, Theme theme);
}
