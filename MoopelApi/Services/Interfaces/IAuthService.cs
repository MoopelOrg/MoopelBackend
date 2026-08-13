using MoopelApi.Models;

using MoopelObjects.Dto;
using MoopelObjects.Requests;
using MoopelObjects.Results;

namespace MoopelApi.Services.Interfaces;

public interface IAuthService
{
    public Task<User?> Register(RegistrationRequest request);
    public Task<LoginResult?> Login(LoginRequest request, RequestContext context, CancellationToken cancelToken);
    public Task<LoginResult?> GuestLogin(RequestContext context, CancellationToken cancelToken);
    public Task<bool> LogoutUser(int userId);


    public Task<GroupAccess> GetUserGroupAccess(User user, Group group);
    public Task<List<GroupAccess>> GetGroupAccess(Group group);

    public Task<bool> CanUserUpdateTheme(User user, Theme theme);
}
