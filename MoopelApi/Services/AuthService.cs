using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MoopelApi.Models;
using MoopelApi.Models.Configuration;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Enums;
using MoopelApi.Models.Requests;
using MoopelApi.Models.Results;
using MoopelApi.Services.Interfaces;

using static MoopelApi.ConstantValues.Database;

namespace MoopelApi.Services;

public class AuthService : IAuthService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly INotificationService _notificationService;
    private readonly DatabaseConnection _connection;

    public AuthService(IOptions<JwtSettings> jwtSettings,
                       IPasswordHasher<User> passwordHasher,
                       ILogger<AuthService> logger,
                       INotificationService notificationService,
                       DatabaseConnection databaseConnection)
    {
        _jwtSettings = jwtSettings.Value;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _notificationService = notificationService;
        _connection = databaseConnection;


        if (_jwtSettings.ExpiryMinutes <= 0)
            throw new ArgumentException("ExpiryMinutes must be greater than 0", nameof(jwtSettings));
    }

    public async Task<User?> Register(RegistrationRequest request)
    {
        try
        {
            _logger.LogInformation("Attempting to register user with username: {Username}", request.Username);
            User startingUser = new()
            {
                UserId = 0,
                Username = request.Username,
                PasswordHash = string.Empty,
                Role = UserRole.Standard,
                Deactivated = false,
                CreatedAtUtc = DateTime.UtcNow,
                ThemeId = null
            };
            string hashedPassword = HashPassword(startingUser, request.Password);
            startingUser.PasswordHash = hashedPassword;

            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();
            SqlCommand command = connection.CreateCommand();

            command.CommandText =
            @$"
                INSERT INTO {UsersTable.SelectName}
                ({UserNameField.SelectName}, {UserPasswordHashField.SelectName}, {UserRoleField.SelectName}, {UserDeactivatedField.SelectName})     
                
                OUTPUT INSERTED.{UserIdField.SelectName};

                VALUES (@Username, @PasswordHash, @Role, @Deactivated);
            ";

            command.Parameters.Add(UserNameField.GenerateParameter("@Username", request.Username));
            command.Parameters.Add(UserPasswordHashField.GenerateParameter("@PasswordHash", hashedPassword));
            command.Parameters.Add(UserRoleField.GenerateParameter("@Role", startingUser.Role.ToString()));
            command.Parameters.Add(UserDeactivatedField.GenerateParameter("@Deactivated", startingUser.Deactivated));

            object? insertedUserIdObject = await command.ExecuteScalarAsync();
            if (insertedUserIdObject == null || insertedUserIdObject == DBNull.Value)
            {
                throw new InvalidOperationException("Failed to insert user [Failed to retrieve inserted user ID].");
            }
            int insertedUserId = Convert.ToInt32(insertedUserIdObject);

            _logger.LogInformation("Successfully registered user with username: {Username} and assigned UserId: {UserId}",
                request.Username, insertedUserId);
            return await GetUserById(insertedUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during registration for username: {Username}", request.Username);
            return null;
        }
    }

    public async Task<User?> GetUserById(int id)
    {
        throw new NotImplementedException();
    }

    private string HashPassword(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }
    private string GenerateToken(User user)
    {
        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email ?? ""),
            new(ClaimTypes.Role, user.Role.ToString()),
            // Iat stands for Issued At
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        ];
        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_jwtSettings.JwtKey));

        SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha512);

        JwtSecurityToken tokenDescriptor = new(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: creds
            );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
    private bool VerifyPassword(User user, string password)
    {
        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Success;
    }

    public Task<LoginResult?> Login(LoginRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeactivateUser(int id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> LogoutUser(int id)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetUserByName(string name)
    {
        throw new NotImplementedException();
    }

    public Task<GroupAccess> GetGroupAccess(User user, Group group)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CanUserUpdateTheme(User user, Theme theme)
    {
        throw new NotImplementedException();
    }
}
