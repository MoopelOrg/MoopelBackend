using Microsoft.Data.SqlClient;

using MoopelApi.Helpers;
using MoopelApi.Models.Configuration;
using MoopelApi.Services.Interfaces;

using MoopelObjects.Dto;
using MoopelObjects.Exceptions;
using MoopelObjects.Requests.Creation;
using MoopelObjects.Requests.Update;

using static MoopelApi.ConstantValues.Database;

namespace MoopelApi.Services;

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly DatabaseConnection _connection;
    private readonly INotificationService _notificationService;

    public UserService(ILogger<UserService> logger, DatabaseConnection connection, INotificationService notificationService)
    {
        _logger = logger;
        _connection = connection;
        _notificationService = notificationService;
    }

    public async Task<List<User>> GetUsers(CancellationToken cancelToken)
    {
        _logger.LogInformation("Getting all users");
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync(cancelToken);

            command = connection.CreateCommand();

            command.CommandText =
                $"""

                                 {UsersTable.GenerateSelect()}
                             
                 """;

            await using SqlDataReader reader = await command.ExecuteReaderAsync(cancelToken);

            List<User> users = [];

            while (await reader.ReadAsync(cancelToken))
            {
                users.Add(DtoReaders.ReadUser(reader));
            }

            _logger.LogInformation("Found [{Count}] users", users.Count);
            return users;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Request to get all users was canceled");
            return [];
        }
        catch (Exception ex)
        {

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting Users",
                ex.Message,
                command: command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<User?> GetUserFromUserId(int userId)
    {
        _logger.LogInformation("Getting user[{Id}]", userId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
                $"""

                                 {UsersTable.GenerateSelect()}
                                 WHERE {UserIdField.SelectName} = @UserId
                             
                 """;
            command.Parameters.Add(UserIdField.GenerateParameter("@UserId", userId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                User user = DtoReaders.ReadUser(reader);
                _logger.LogInformation("Found user using [{Id}]", userId);
                return user;
            }
            else
            {
                _logger.LogInformation("Found no rows for user[{Id}]", userId);
                return null;
            }
        }
        catch (Exception ex)
        {

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting User",
                ex.Message,
                [
                    ("UserId", userId)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<User?> GetUserFromUserName(string userName)
    {
        _logger.LogInformation("Getting user[{Name}]", userName);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
                $"""

                                 {UsersTable.GenerateSelect()}
                                 WHERE {UserNameField.SelectName} = @UserName
                             
                 """;
            command.Parameters.Add(UserNameField.GenerateParameter("@UserName", userName));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                User user = DtoReaders.ReadUser(reader);
                _logger.LogInformation("Found user using [{Name}]", userName);
                return user;
            }
            else
            {
                _logger.LogInformation("Found no rows for user[{Name}]", userName);
                return null;
            }
        }
        catch (Exception ex)
        {

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting User",
                ex.Message,
                [
                    ("UserName", userName)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<User?> UpdateUser(UserUpdateRequest updateRequest, int userId)
    {
        ArgumentNullException.ThrowIfNull(updateRequest);

        _logger.LogInformation("Updating user[{Id}]", userId);

        SqlCommand? command = null;
        SqlTransaction? transaction = null;

        User? user = await GetUserFromUserId(userId)
            ?? throw new NotFoundException($"User [{userId}] was not found.");

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            transaction = connection.BeginTransaction();

            command = connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText =
                $"""
                    UPDATE {UsersTable.SelectName}
                    SET {UserEmailField.SelectName} = @Email
                    {UsersTable.GenerateOutput()}
                    WHERE {UserIdField.SelectName} = @UserId   
                 """;

            command.Parameters.Add(UserEmailField.GenerateParameter("@Email", updateRequest.Email ?? user.Email));
            command.Parameters.Add(UserIdField.GenerateParameter("@UserId", userId));

            User updatedUser;

            await using (SqlDataReader reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync() is false)
                {
                    throw new InvalidOperationException("Failed to create note.");
                }

                updatedUser = DtoReaders.ReadUser(reader);
            }

            await transaction.CommitAsync();

            return updatedUser;
        }
        catch (Exception ex)
        {
            if (transaction is not null && transaction.Connection is not null)
            {
                await transaction.RollbackAsync();
            }

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Updating User",
                ex.Message,
                [
                    ("UserId", userId),
                    ("UpdateRequest", updateRequest)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task DisableUser(int userId)
    {
        _logger.LogInformation("Disabling user[{Id}]", userId);
        SqlCommand? command = null;
        SqlTransaction? transaction = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            transaction = connection.BeginTransaction();

            command = connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText =
                $"""

                                 UPDATE {UsersTable.SelectName}
                                 SET {UserDeactivatedField.SelectName} = 1
                                 WHERE {UserIdField.SelectName} = @UserId
                             
                 """;

            command.Parameters.Add(UserIdField.GenerateParameter("@UserId", userId));

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected != 1)
                throw new Exception($"Rows affected[{rowsAffected}] did not match expected 1.");

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            if (transaction is not null && transaction.Connection is not null)
            {
                await transaction.RollbackAsync();
            }

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Disabling User",
                ex.Message,
                [
                    ("UserId", userId)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<UserSettings?> GetSettingsFromUserId(int userId)
    {
        _logger.LogInformation("Getting settings[{UserId}]", userId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
                $"""

                                 {SettingsTable.GenerateSelect()}
                                WHERE {SettingsUserIdField.SelectName} = @UserId
                             
                 """;
            command.Parameters.Add(SettingsUserIdField.GenerateParameter("@UserId", userId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                UserSettings settings = DtoReaders.ReadUserSettings(reader);
                _logger.LogInformation("Found settings using [{UserId}]", userId);
                return settings;
            }
            else
            {
                _logger.LogInformation("Found no rows for settings using [{UserId}]", userId);
                return null;
            }
        }
        catch (Exception ex)
        {

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting User Settings",
                ex.Message,
                [
                    ("UserId", userId)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<UserSettings?> GetSettingsFromSettingId(int settingsId)
    {
        _logger.LogInformation("Getting settings[{SettingsId}]", settingsId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
                $"""

                                 {SettingsTable.GenerateSelect()}
                                 WHERE {SettingsIdField.SelectName} = @SettingsId
                             
                 """;
            command.Parameters.Add(SettingsIdField.GenerateParameter("@SettingsId", settingsId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                UserSettings settings = DtoReaders.ReadUserSettings(reader);
                _logger.LogInformation("Found settings using [{SettingsId}]", settingsId);
                return settings;
            }
            else
            {
                _logger.LogInformation("Found no rows for [{SettingsId}]", settingsId);
                return null;
            }
        }
        catch (Exception ex)
        {

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting User Settings",
                ex.Message,
                [
                    ("SettingsId", settingsId)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<UserSettings?> UpdateUserSettings(UserSettingsUpdateRequest updateRequest, int settingsId)
    {
        ArgumentNullException.ThrowIfNull(updateRequest);

        _logger.LogInformation("Updating settings[{SettingsId}]", settingsId);

        SqlCommand? command = null;
        SqlTransaction? transaction = null;

        UserSettings existingSettings = await GetSettingsFromSettingId(settingsId)
            ?? throw new NotFoundException($"User settings [{settingsId}] were not found.");

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            transaction = connection.BeginTransaction();

            command = connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText =
                $"""
                    UPDATE {SettingsTable.SelectName}
                    SET {SettingTimezoneField.SelectName} = @Timezone,
                        {SettingDateFormatField.SelectName} = @DateFormat,
                        {SettingsLastUpdatedAtUtcField.SelectName} = SYSUTCDATETIME()
                    {SettingsTable.GenerateOutput()}
                    WHERE {SettingsIdField.SelectName} = @SettingsId
                  """;

            command.Parameters.Add(SettingTimezoneField.GenerateParameter("@Timezone", updateRequest.Timezone ?? existingSettings.Timezone));
            command.Parameters.Add(SettingDateFormatField.GenerateParameter("@DateFormat", updateRequest.DateFormat ?? existingSettings.DateFormat));
            command.Parameters.Add(SettingsIdField.GenerateParameter("@SettingsId", settingsId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                UserSettings updatedSettings = DtoReaders.ReadUserSettings(reader);
                await transaction.CommitAsync();
                return updatedSettings;
            }

            throw new InvalidOperationException("Failed to update user settings.");
        }
        catch (Exception ex)
        {
            if (transaction is not null && transaction.Connection is not null)
            {
                await transaction.RollbackAsync();
            }

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Updating UserSettings",
                ex.Message,
                [
                    ("SettingsId", settingsId),
                    ("UpdateRequest", updateRequest)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<UserSettings> CreateUserSettings(UserSettingsCreateRequest createRequest, int userId)
    {
        ArgumentNullException.ThrowIfNull(createRequest, nameof(createRequest));

        _logger.LogInformation("Creating user settings for user[{Id}]", userId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
                $"""

                                 INSERT INTO {SettingsTable.SelectName}
                                ({SettingsUserIdField.SelectName}, {SettingTimezoneField.SelectName}, {SettingDateFormatField.SelectName})
                                {SettingsTable.GenerateOutput()}
                                 VALUES(@UserId, @Timezone, @DateFormat)
                             
                 """;

            command.Parameters.Add(UserIdField.GenerateParameter("@UserId", userId));
            command.Parameters.Add(SettingTimezoneField.GenerateParameter("@Timezone", createRequest.Timezone));
            command.Parameters.Add(SettingDateFormatField.GenerateParameter("@DateFormat", createRequest.DateFormat));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return DtoReaders.ReadUserSettings(reader);
            }

            throw new InvalidOperationException("Failed to create user settings.");
        }
        catch (Exception ex)
        {
            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Creating UserSettings",
                ex.Message,
                [
                    ("UserId", userId),
                    ("UserSettingsCreateRequest", createRequest)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }
}
