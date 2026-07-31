using Microsoft.Data.SqlClient;

using MoopelApi.Helpers;
using MoopelApi.Models;
using MoopelApi.Models.Configuration;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Dto.Read;
using MoopelApi.Models.Enums;
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

    public async Task<Session> GenerateSession(RequestContext context, UserRead user, Login? login, string token)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(user);

        _logger.LogInformation("Generating session for {User} at {Context}", user.Username, context);
        SqlCommand? command = null;
        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
            $"""
                INSERT INTO {SessionsTable.SelectName}
                ({SessionUserIdField.SelectName}, {SessionLoginIdField.SelectName}, {SessionTokenField.SelectName}, {SessionIpIssuedToField.SelectName}, {SessionIsGuestField.SelectName})
                {SessionsTable.GenerateOutput()}
                VALUES (@UserId, @LoginId, @Token, @IP, @IsGuest);
            """;

            int? userId = user.UserId;

            if (user.Role == Roles.Guest)
                userId = null;

            command.Parameters.Add(SessionUserIdField.GenerateParameter("@UserId", userId));
            command.Parameters.Add(SessionLoginIdField.GenerateParameter("@LoginId", login?.LoginId));
            command.Parameters.Add(SessionTokenField.GenerateParameter("@Token", token));
            command.Parameters.Add(SessionIpIssuedToField.GenerateParameter("@IP", context.IpAddress));
            command.Parameters.Add(SessionIsGuestField.GenerateParameter("@IsGuest", user.Role == Roles.Guest));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                Session session = DtoReaders.ReadSession(reader);
                _logger.LogInformation("Generated Session {Id}", session.SessionId);

                return session;
            }
            else
            {
                throw new("Failed to read generated session");
            }
        }
        catch (Exception ex)
        {
            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Generating Session",
                ex.Message,
                [
                    ("Context", context),
                    ("User", user),
                    ("Login", login),
                    ("Token", token)
                ],
                command: command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);
            throw;
        }
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
                $"""

                                 SELECT {SessionsTable.GenerateSelect()}
                                 WHERE {SessionIdField.SelectName} = @SessionId
                             
                 """;
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

    public async Task<List<Session>> GetUserSessions(int userId, CancellationToken cancelToken)
    {
        _logger.LogInformation("Getting sessions for user [{UserId}]", userId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync(cancelToken);

            command = connection.CreateCommand();

            command.CommandText =
                $"""

                                 SELECT {SessionsTable.GenerateSelect()}
                                 WHERE {SessionUserIdField.SelectName} = @UserId
                             
                 """;
            command.Parameters.Add(SessionUserIdField.GenerateParameter("@UserId", userId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync(cancelToken);

            List<Session> sessions = [];

            while (await reader.ReadAsync(cancelToken))
            {
                sessions.Add(DtoReaders.ReadSession(reader));
            }

            _logger.LogInformation("Found {Count} guest sessions for user [{UserId}]", sessions.Count, userId);

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

    public async Task<List<Session>> GetGuestUserSessions(CancellationToken cancelToken)
    {
        _logger.LogInformation("Getting Guest Sessions.");
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync(cancelToken);

            command = connection.CreateCommand();

            command.CommandText =
            $"""
                SELECT {SessionsTable.GenerateSelect()}
                WHERE {SessionIsGuestField.SelectName} = 1
            """;

            await using SqlDataReader reader = await command.ExecuteReaderAsync(cancelToken);

            List<Session> sessions = [];

            while (await reader.ReadAsync(cancelToken))
            {
                sessions.Add(DtoReaders.ReadSession(reader));
            }

            _logger.LogInformation("Found {Count} guest sessions", sessions.Count);

            return sessions;
        }
        catch (Exception ex)
        {
            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting User Sessions",
                ex.Message,
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
                $"""

                                 UPDATE {SessionsTable.SelectName}
                                 SET {SessionRevokedField.SelectName} = @Revoked
                                 WHERE {SessionRevokedField.SelectName} = @NotRevoked
                             
                 """;
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

    public async Task RevokeSession(int sessionId)
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
                $"""

                                 UPDATE {SessionsTable.SelectName}
                                 SET {SessionRevokedField.SelectName} = @Revoked
                                 WHERE {SessionIdField.SelectName} = @SessionId
                             
                 """;
            command.Parameters.Add(SessionRevokedField.GenerateParameter("@Revoked", true));
            command.Parameters.Add(SessionIdField.GenerateParameter("@SessionId", sessionId));

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected != 1)
            {
                throw new InvalidOperationException($"Revoked [{rowsAffected}] sessions when only one session should have been revoked.");
            }

            await transaction.CommitAsync();
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