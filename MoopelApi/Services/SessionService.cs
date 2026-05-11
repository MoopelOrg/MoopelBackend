using Microsoft.Data.SqlClient;
using MoopelApi.Helpers;
using MoopelApi.Models.Configuration;
using MoopelApi.Models.Dto;
using MoopelApi.Services.Interfaces;

using static MoopelApi.ConstantValues.Database;

namespace MoopelApi.Services;

public class SessionService : ISessionService
{
    private readonly ILogger<SessionService> _logger;
    private readonly INotificationService _notificationService;
    private readonly DatabaseConnection _connection;

    public SessionService(ILogger<SessionService> logger, INotificationService notificationService, DatabaseConnection connection)
    {
        _logger = logger;
        _notificationService = notificationService;
        _connection = connection;
    }

    public async Task<Session> GetSessionById(int sessionId)
    {
        _logger.LogInformation("Getting session [{SessionId}]", sessionId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
            @$"
                SELECT {SessionsTable.GenerateSelect()}
                WHERE {SessionIdField.SelectName} = @SessionId
            ";
            command.Parameters.Add(SessionIdField.GenerateParameter("@SessionId", sessionId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                Session session = DtoReaders.ReadSession(reader);
                _logger.LogInformation("Found [{Session}] for [{SessionId}]", session, sessionId);
                return session;
            }

            _logger.LogInformation("No session found for [{SessionId}]", sessionId);
            throw new InvalidOperationException($"No session found for SessionId [{sessionId}].");
        }
        catch (Exception ex)
        {
            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting Session",
                ex.Message,
                [
                    ("SessionId", sessionId)
                ],
                command: command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<List<Session>> GetUserSessions(int userId)
    {
        _logger.LogInformation("Getting sessions for user [{UserId}]", userId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
            @$"
                SELECT {SessionsTable.GenerateSelect()}
                WHERE {SessionUserIdField.SelectName} = @UserId
            ";
            command.Parameters.Add(SessionUserIdField.GenerateParameter("@UserId", userId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            List<Session> sessions = [];

            while (await reader.ReadAsync())
            {
                sessions.Add(DtoReaders.ReadSession(reader));
            }

            if (sessions.Count is 0)
            {
                _logger.LogInformation("No sessions found for user [{UserId}]", userId);
            }

            return sessions;
        }
        catch (Exception ex)
        {
            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting User Sessions",
                ex.Message,
                [
                    ("UserId", userId)
                ],
                command: command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<bool> RevokeAllSessions()
    {
        _logger.LogInformation("Revoking all sessions");
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
                UPDATE {SessionsTable.SelectName}
                SET {SessionRevokedField.SelectName} = @Revoked
                WHERE {SessionRevokedField.SelectName} = @NotRevoked
            ";
            command.Parameters.Add(SessionRevokedField.GenerateParameter("@Revoked", true));
            command.Parameters.Add(SessionRevokedField.GenerateParameter("@NotRevoked", false));

            int rowsAffected = await command.ExecuteNonQueryAsync();

            await transaction.CommitAsync();

            _logger.LogInformation("Revoked [{RowsAffected}] sessions", rowsAffected);

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Revoking All Sessions",
                ex.Message,
                [],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<bool> RevokeSession(int sessionId)
    {
        _logger.LogInformation("Revoking session [{SessionId}]", sessionId);
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
                UPDATE {SessionsTable.SelectName}
                SET {SessionRevokedField.SelectName} = @Revoked
                WHERE {SessionIdField.SelectName} = @SessionId
            ";
            command.Parameters.Add(SessionRevokedField.GenerateParameter("@Revoked", true));
            command.Parameters.Add(SessionIdField.GenerateParameter("@SessionId", sessionId));

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected > 1)
            {
                throw new InvalidOperationException($"Revoked [{rowsAffected}] sessions when only one session should have been revoked.");
            }

            await transaction.CommitAsync();

            return rowsAffected == 1;
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Revoking Session",
                ex.Message,
                [
                    ("SessionId", sessionId)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }
}