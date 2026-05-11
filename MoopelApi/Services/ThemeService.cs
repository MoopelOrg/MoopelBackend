using Microsoft.Data.SqlClient;
using MoopelApi.Helpers;
using MoopelApi.Models.Configuration;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;
using MoopelApi.Services.Interfaces;

using static MoopelApi.ConstantValues.Database;

namespace MoopelApi.Services;

public class ThemeService : IThemeService
{
    private readonly ILogger<ThemeService> _logger;
    private readonly INotificationService _notificationService;
    private readonly DatabaseConnection _connection;

    public ThemeService(ILogger<ThemeService> logger, INotificationService notificationService, DatabaseConnection connection)
    {
        _logger = logger;
        _notificationService = notificationService;
        _connection = connection;
    }

    public async Task<Theme> CreateTheme(ThemeCreateRequest request, User creatingUser)
    {
        _logger.LogInformation("Creating theme for user [{UserId}]", creatingUser.UserId);
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
            @$"
                INSERT INTO {ThemesTable.SelectName}
                ({ThemeCreatorDescriptorField.SelectName},
                 {ThemeNameField.SelectName},
                 {ThemeDescriptionField.SelectName})
                OUTPUT INSERTED.{ThemesTable.GetFields()}
                VALUES
                (@CreatorUser, @Name, @Description, @Settings)
            ";
            command.Parameters.Add(ThemeCreatorDescriptorField.GenerateParameter("@CreatorUser", GenerateUserDescriptor(creatingUser)));
            command.Parameters.Add(ThemeNameField.GenerateParameter("@Name", request.Name));
            command.Parameters.Add(ThemeDescriptionField.GenerateParameter("@Description", request.Description));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                Theme theme = DtoReaders.ReadTheme(reader);
                await transaction.CommitAsync();
                return theme;
            }
            else
            {
                throw new InvalidOperationException("Failed to create theme.");
            }
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Creating Theme",
                ex.Message,
                [
                    ("CreatingUser", creatingUser),
                    ("CreateRequest", request)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task DeleteTheme(int themeId)
    {
        _logger.LogInformation("Deleting theme [{ThemeId}]", themeId);
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
            @$"
                DELETE FROM {ThemesTable.SelectName}
                WHERE {ThemeIdField.SelectName} = @ThemeId
            ";
            command.Parameters.Add(ThemeIdField.GenerateParameter("@ThemeId", themeId));

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected > 1)
            {
                throw new InvalidOperationException($"Deleted [{rowsAffected}] themes when only one theme should have been deleted.");
            }

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Deleting Theme",
                ex.Message,
                [
                    ("ThemeId", themeId)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<List<Theme>> GetAllThemes(CancellationToken cancelToken)
    {
        _logger.LogInformation("Getting all themes");
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync(cancelToken);

            command = connection.CreateCommand();

            command.CommandText =
            @$"
                SELECT {ThemesTable.GenerateSelect()}
            ";

            await using SqlDataReader reader = await command.ExecuteReaderAsync(cancelToken);

            List<Theme> themes = [];

            while (await reader.ReadAsync(cancelToken))
            {
                themes.Add(DtoReaders.ReadTheme(reader));
            }

            if (themes.Count is 0)
            {
                _logger.LogInformation("No themes found");
            }

            return themes;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Request to get all themes was canceled");
            return [];
        }
        catch (Exception ex)
        {
            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting All Themes",
                ex.Message,
                [],
                command: command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<Theme?> GetThemeById(int themeId)
    {
        _logger.LogInformation("Getting theme [{ThemeId}]", themeId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
            @$"
                SELECT {ThemesTable.GenerateSelect()}
                WHERE {ThemeIdField.SelectName} = @ThemeId
            ";
            command.Parameters.Add(ThemeIdField.GenerateParameter("@ThemeId", themeId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                Theme theme = DtoReaders.ReadTheme(reader);
                _logger.LogInformation("Found [{Theme}] for [{ThemeId}]", theme, themeId);
                return theme;
            }

            _logger.LogInformation("No theme found for [{ThemeId}]", themeId);
            return null;
        }
        catch (Exception ex)
        {
            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting Theme",
                ex.Message,
                [
                    ("ThemeId", themeId)
                ],
                command: command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<Theme> UpdateTheme(int themeId, ThemeUpdateRequest request)
    {
        _logger.LogInformation("Updating theme [{ThemeId}]", themeId);
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
            @$"
                UPDATE {ThemesTable.SelectName}
                SET {ThemeNameField.SelectName} = @Name,
                    {ThemeDescriptionField.SelectName} = @Description
                OUTPUT INSERTED.{ThemesTable.GetFields()}
                WHERE {ThemeIdField.SelectName} = @ThemeId
            ";
            command.Parameters.Add(ThemeNameField.GenerateParameter("@Name", request.Name));
            command.Parameters.Add(ThemeDescriptionField.GenerateParameter("@Description", request.Description));
            command.Parameters.Add(ThemeIdField.GenerateParameter("@ThemeId", themeId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync() is false)
            {
                throw new InvalidOperationException($"No theme found for ThemeId [{themeId}].");
            }

            Theme theme = DtoReaders.ReadTheme(reader);

            if (await reader.ReadAsync())
            {
                throw new InvalidOperationException($"Updated more than one theme for ThemeId [{themeId}].");
            }

            await transaction.CommitAsync();

            return theme;
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Updating Theme",
                ex.Message,
                [
                    ("ThemeId", themeId),
                    ("UpdateRequest", request)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    private static string GenerateUserDescriptor(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return $"User [{user.UserId}] - {user.Username}";
    }
}