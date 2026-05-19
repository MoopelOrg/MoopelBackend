using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using MoopelApi.Helpers;
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
    private readonly ISessionService _sessionService;
    private readonly DatabaseConnection _connection;

    public AuthService(IOptions<JwtSettings> jwtSettings,
                       IPasswordHasher<User> passwordHasher,
                       ILogger<AuthService> logger,
                       INotificationService notificationService,
                       ISessionService sessionService,
                       DatabaseConnection databaseConnection)
    {
        ArgumentNullException.ThrowIfNull(jwtSettings, nameof(jwtSettings));
        _jwtSettings = jwtSettings.Value;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _notificationService = notificationService;
        _connection = databaseConnection;
        _sessionService = sessionService;


        if (_jwtSettings.ExpiryMinutes <= 0)
            throw new ArgumentException("ExpiryMinutes must be greater than 0", nameof(jwtSettings));
    }

    public async Task<User?> Register(RegistrationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

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
                OUTPUT INSERTED.{UserIdField.SelectName}
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
        try
        {
            _logger.LogInformation("Getting user by id [{UserId}]", id);

            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            await using SqlCommand command = connection.CreateCommand();

            command.CommandText =
            @$"
                SELECT {UsersTable.GetFields()}
                FROM {UsersTable.SelectName}
                WHERE {UserIdField.SelectName} = @UserId;
            ";

            command.Parameters.Add(UserIdField.GenerateParameter("@UserId", id));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }
            else
            {
                User user = DtoReaders.ReadUser(reader);
                return user;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by id [{UserId}]", id);
            return null;
        }
    }

    public async Task<User?> GetUserByName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        try
        {
            _logger.LogInformation("Getting user by username [{Username}]", name);

            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            await using SqlCommand command = connection.CreateCommand();

            command.CommandText =
            @$"
                SELECT {UsersTable.GetFields()}
                FROM {UsersTable.SelectName}
                WHERE {UserNameField.SelectName} = @Username;
            ";

            command.Parameters.Add(UserNameField.GenerateParameter("@Username", name));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return DtoReaders.ReadUser(reader);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by username [{Username}]", name);
            return null;
        }
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

    public async Task<LoginResult?> Login(LoginRequest request, CancellationToken cancelToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        try
        {
            _logger.LogInformation("Attempting login for username [{Username}]", request.Username);

            User? user = await GetUserByName(request.Username);

            if (user is null)
            {
                _logger.LogWarning("Login failed for username [{Username}]: user not found", request.Username);
                return null;
            }

            if (user.Deactivated)
            {
                _logger.LogWarning("Login failed for username [{Username}]: user is deactivated", request.Username);
                return null;
            }

            if (!VerifyPassword(user, request.Password))
            {
                _logger.LogWarning("Login failed for username [{Username}]: invalid password", request.Username);
                return null;
            }

            string token = GenerateToken(user);

            _logger.LogInformation("Login succeeded for user [{UserId}]", user.UserId);

            return new LoginResult
            {
                User = user.ToRead(),
                Token = token
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Login was canceled for username [{Username}]", request.Username);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username [{Username}]", request.Username);
            return null;
        }
    }
    public async Task<LoginResult?> GuestLogin(CancellationToken cancelToken)
    {
        cancelToken.ThrowIfCancellationRequested();

        User guestUser = new()
        {
            UserId = 0,
            Username = "Guest",
            PasswordHash = string.Empty,
            Email = null,
            Role = UserRole.Guest,
            Deactivated = false,
            CreatedAtUtc = DateTime.UtcNow,
            ThemeId = null
        };

        string token = GenerateToken(guestUser);

        LoginResult result = new()
        {
            User = guestUser.ToRead(),
            Token = token
        };

        return result;
    }

    public async Task<bool> DeactivateUser(int id)
    {
        try
        {
            _logger.LogInformation("Deactivating user [{UserId}]", id);

            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            await using SqlCommand command = connection.CreateCommand();

            command.CommandText =
            @$"
                UPDATE {UsersTable.SelectName}
                SET {UserDeactivatedField.SelectName} = @Deactivated
                WHERE {UserIdField.SelectName} = @UserId;
            ";

            command.Parameters.Add(UserDeactivatedField.GenerateParameter("@Deactivated", true));
            command.Parameters.Add(UserIdField.GenerateParameter("@UserId", id));

            int rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected == 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user [{UserId}]", id);
            return false;
        }
    }

    public async Task<bool> LogoutUser(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> CanUserUpdateTheme(User user, Theme theme)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentNullException.ThrowIfNull(theme, nameof(theme));

        if (user.Role == UserRole.Admin)
        {
            return true;
        }

        if (FormatHelper.IsDescriptorForUser(theme.CreatorDescriptor, user))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task<GroupAccess> GetUserGroupAccess(User user, Group group)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentNullException.ThrowIfNull(group, nameof(group));

        try
        {
            _logger.LogInformation("Getting user group access for {UserId} - {GroupId}", user.UserId, group.GroupId);

            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            await using SqlCommand command = connection.CreateCommand();

            command.CommandText =
            @$"
                SELECT {GroupUsersTable.GenerateSelect()}
                WHERE {GroupUsersUserIdField.SelectName} = @UserId
                AND {GroupUsersGroupIdField.SelectName} = @GroupId;
            ";
            command.Parameters.Add(GroupUsersUserIdField.GenerateParameter("@UserId", user.UserId));
            command.Parameters.Add(GroupUsersUserIdField.GenerateParameter("@GroupId", group.GroupId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                GroupUser groupUser = DtoReaders.ReadGroupUser(reader);
                GroupAccess access = GroupAccess.GetAccessFromDto(groupUser);
                return access;
            }
            else
            {
                GroupAccess access = GroupAccess.GetNoAccess(user.UserId, group.GroupId);
                return access;
            }
        }
        catch (Exception ex)
        {
#error CHANGE LOG
            _logger.LogError(ex, "Error getting group access by username [{Username}]", name);
            GroupAccess access = GroupAccess.GetNoAccess(user.UserId, group.GroupId);
            return access;
        }
    }

    public async Task<List<GroupAccess>> GetGroupAccess(Group group)
    {
        throw new NotImplementedException();
    }
}
