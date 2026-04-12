using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests;
using MoopelApi.Models.Results;

namespace MoopelApi.Services.Interfaces;

public interface IAuthService
{
    Task<User?> Register(RegistrationRequest request);
    Task<LoginResult?> Login(LoginRequest request);
    Task<bool> DeactivateUser(int id);
    Task<bool> LogoutUser(int id);

    Task<User?> GetUserById(int id);
    Task<User?> GetUserByName(string name);
}
