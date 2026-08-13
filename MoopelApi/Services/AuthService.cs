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
using MoopelApi.Services.Interfaces;

using MoopelObjects.Dto;
using MoopelObjects.Enums;
using MoopelObjects.Requests;
using MoopelObjects.Results;

using static MoopelApi.ConstantValues.Database;

namespace MoopelApi.Services;

public class AuthService : IAuthService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly INotificationService _notificationService;
    private readonly ISessionService _sessionService;
    private readonly IUserService _userService;
    private readonly DatabaseConnection _connection;

    public AuthService(IOptions<JwtSettings> jwtSettings,
                       IPasswordHasher<User> passwordHasher,
                       ILogger<AuthService> logger,
                       IUserService userService,
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
        _userService = userService;


        if (_jwtSettings.ExpiryMinutes <= 0)
            throw new ArgumentException("ExpiryMinutes must be greater than 0", nameof(jwtSettings));
    }

    public async Task<User?> Register(RegistrationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        try
        {
            _logger.LogInformation("Attempting to register user with username: {Username}", request.Username);

            if (request.Username.StartsWith("Guest_"))
            {
                throw new("Can't register an account starting with Guest_");
            }

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
            $"""
                INSERT INTO {UsersTable.SelectName}
                ({UserNameField.SelectName}, {UserPasswordHashField.SelectName}, {UserRoleField.SelectName}, {UserDeactivatedField.SelectName})
                OUTPUT INSERTED.{UserIdField.SelectName}
                VALUES (@Username, @PasswordHash, @Role, @Deactivated);
            """;

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
            return await _userService.GetUserFromUserId(insertedUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during registration for username: {Username}", request.Username);
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

    public async Task<LoginResult?> Login(LoginRequest request, RequestContext context, CancellationToken cancelToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        string? token = null;
        User? user;
        string? error = null;
        LoginStatus status = LoginStatus.Failed;

        try
        {
            _logger.LogInformation("Attempting login for username [{Username}]", request.Username);

            user = await _userService.GetUserFromUserName(request.Username);

            // Validate Login Request
            if (user is null)
            {
                error = ConstantValues.ErrorMessages.InvalidLogin(request.Username);
                _logger.LogWarning("Login failed for username [{Username}]: user not found", request.Username);
            }
            else
            {
                if (user.Deactivated)
                {
                    error = $"Login failed for username [{user.Username}]: user is deactivated";
                    _logger.LogWarning("Login failed for username [{Username}]: user is deactivated", request.Username);
                }
                else if (!VerifyPassword(user, request.Password))
                {
                    error = ConstantValues.ErrorMessages.InvalidLogin(request.Username);
                    _logger.LogWarning("Login failed for username [{Username}]: invalid password", request.Username);
                }
                else
                {
                    token = GenerateToken(user);
                    status = LoginStatus.Successful;
                }
            }

            // Store Login Result
            SqlCommand? command = null;
            LoginResult? loginResult = null;
            Login? login = null;
            Session? session = null;
            try
            {
                await using SqlConnection connection = new(_connection.ConnectionString);
                await connection.OpenAsync(cancelToken);

                command = connection.CreateCommand();

                command.CommandText =
                $"""
                    INSERT INTO {LoginsTable.SelectName}
                    ({LoginUserNameField.SelectName}, {LoginStatusField.SelectName})
                    {LoginsTable.GenerateOutput()}
                    VALUES (@Username, @Status);
                """;
                command.Parameters.Add(LoginUserNameField.GenerateParameter("@Username", request.Username));
                command.Parameters.Add(LoginStatusField.GenerateParameter("@Status", status));

                await using SqlDataReader reader = await command.ExecuteReaderAsync(cancelToken);

                if (await reader.ReadAsync(cancelToken))
                {
                    login = DtoReaders.ReadLogin(reader);
                }
                else
                {
                    throw new("Failed to read created login");
                }
            }
            catch (Exception ex)
            {
                string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Generating Login",
                ex.Message,
                [
                    ("Token", token),
                            ("Username", request.Username)
                ],
                command: command);
            }

            if (user is not null && login is not null && token is not null && status == LoginStatus.Successful)
            {
                session = await _sessionService.GenerateSession(context, user.ToRead(), login, token);
                _logger.LogInformation("Login succeeded for user [{UserId}]", user!.UserId);
                loginResult = new(token, error, user?.ToRead())
                {
                    Login = login,
                    Session = session,
                };
            }
            else
            {
                loginResult = new(token, error, user?.ToRead());
            }

            return loginResult;
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

    public async Task<LoginResult?> GuestLogin(RequestContext context, CancellationToken cancelToken)
    {
        cancelToken.ThrowIfCancellationRequested();

        User guestUser = new()
        {
            UserId = 0,
            Username = $"Guest_{Guid.NewGuid()}",
            PasswordHash = string.Empty,
            Email = null,
            Role = UserRole.Guest,
            Deactivated = false,
            CreatedAtUtc = DateTime.UtcNow,
            ThemeId = null
        };

        string token = GenerateToken(guestUser);

        Session session = await _sessionService.GenerateSession(context, guestUser.ToRead(), null, token);

        LoginResult result = new()
        {
            User = guestUser.ToRead(),
            Token = token,
            Session = session
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
                $"""

                                 UPDATE {UsersTable.SelectName}
                                 SET {UserDeactivatedField.SelectName} = @Deactivated
                                 WHERE {UserIdField.SelectName} = @UserId;
                             
                 """;

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

        return user.Role == UserRole.Admin || FormatHelper.IsDescriptorForUser(theme.CreatorDescriptor, user);
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
            $"""

                             {GroupUsersTable.GenerateSelect()}
                             WHERE {GroupUsersUserIdField.SelectName} = @UserId
                             AND {GroupUsersGroupIdField.SelectName} = @GroupId;
                         
             """;
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
            _logger.LogError(ex, "Error getting group access by [{User}] [{Group}]", user.Username, group.GroupId);
            GroupAccess access = GroupAccess.GetNoAccess(user.UserId, group.GroupId);
            return access;
        }
    }

    public async Task<List<GroupAccess>> GetGroupAccess(Group group)
    {
        throw new NotImplementedException();
    }
}
