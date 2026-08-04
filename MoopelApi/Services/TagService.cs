using Microsoft.Data.SqlClient;

using MoopelApi.Helpers;
using MoopelApi.Models.Configuration;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;
using MoopelApi.Services.Interfaces;

using static MoopelApi.ConstantValues.Database;

namespace MoopelApi.Services;

public class TagService : ITagService
{
    private readonly ILogger<TagService> _logger;
    private readonly INotificationService _notificationService;
    private readonly DatabaseConnection _connection;

    public TagService(ILogger<TagService> logger, INotificationService notificationService, DatabaseConnection connection)
    {
        _logger = logger;
        _notificationService = notificationService;
        _connection = connection;
    }

    public async Task<List<UserTag>> GetUserTags(int userId)
    {
        _logger.LogInformation("Getting user tags for user [{UserId}]", userId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
                $"""

                                 {UserTagsTable.GenerateSelect()}
                                 WHERE {UserTagUserIdField.SelectName} = @UserId
                             
                 """;
            command.Parameters.Add(UserTagUserIdField.GenerateParameter("@UserId", userId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            List<UserTag> tags = [];

            while (await reader.ReadAsync())
            {
                tags.Add(DtoReaders.ReadUserTag(reader));
            }

            if (tags.Count is 0)
            {
                _logger.LogInformation("No user tags found for user [{UserId}]", userId);
            }

            return tags;
        }
        catch (Exception ex)
        {
            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting User Tags",
                ex.Message,
                [
                    ("UserId", userId)
                ],
                command: command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<UserTag> GetUserTagById(int tagId)
    {
        _logger.LogInformation("Getting user tag [{TagId}]", tagId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
                $"""

                                 {UserTagsTable.GenerateSelect()}
                                 WHERE {UserTagIdField.SelectName} = @TagId
                             
                 """;
            command.Parameters.Add(UserTagIdField.GenerateParameter("@TagId", tagId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                UserTag tag = DtoReaders.ReadUserTag(reader);
                _logger.LogInformation("Found [{UserTag}] for [{TagId}]", tag, tagId);
                return tag;
            }

            _logger.LogInformation("No user tag found for [{TagId}]", tagId);
            throw new InvalidOperationException($"No user tag found for TagId [{tagId}].");
        }
        catch (Exception ex)
        {
            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting User Tag",
                ex.Message,
                [
                    ("TagId", tagId)
                ],
                command: command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<UserTag> CreateUserTag(TagCreateRequest request, int userId)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        _logger.LogInformation("Creating user tag for user [{UserId}]", userId);
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

                                 INSERT INTO {UserTagsTable.SelectName}
                                 ({UserTagUserIdField.SelectName},
                                  {UserTagNameField.SelectName},
                                  {UserTagColorField.SelectName})
                                 OUTPUT INSERTED.{UserTagsTable.GetFields()}
                                 VALUES
                                 (@UserId, @TagName, @TagColor)
                             
                 """;
            command.Parameters.Add(UserTagUserIdField.GenerateParameter("@UserId", userId));
            command.Parameters.Add(UserTagNameField.GenerateParameter("@TagName", request.Name));
            command.Parameters.Add(UserTagColorField.GenerateParameter("@TagColor", request.Color));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync() is false)
            {
                throw new InvalidOperationException("Failed to create user tag.");
            }

            UserTag tag = DtoReaders.ReadUserTag(reader);

            await transaction.CommitAsync();

            return tag;
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Creating User Tag",
                ex.Message,
                [
                    ("UserId", userId),
                    ("CreateRequest", request)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<UserTag?> UpdateUserTag(TagUpdateRequest request, int tagId)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        _logger.LogInformation("Updating user tag [{TagId}]", tagId);
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

                                 UPDATE {UserTagsTable.SelectName}
                                 SET {UserTagNameField.SelectName} = @TagName,
                                     {UserTagColorField.SelectName} = @TagColor
                                 OUTPUT INSERTED.{UserTagsTable.GetFields()}
                                 WHERE {UserTagIdField.SelectName} = @TagId
                             
                 """;
            command.Parameters.Add(UserTagNameField.GenerateParameter("@TagName", request.Name));
            command.Parameters.Add(UserTagColorField.GenerateParameter("@TagColor", request.Color));
            command.Parameters.Add(UserTagIdField.GenerateParameter("@TagId", tagId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync() is false)
            {
                await transaction.CommitAsync();
                return null;
            }

            UserTag tag = DtoReaders.ReadUserTag(reader);

            if (await reader.ReadAsync())
            {
                throw new InvalidOperationException($"Updated more than one user tag for TagId [{tagId}].");
            }

            await transaction.CommitAsync();

            return tag;
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Updating User Tag",
                ex.Message,
                [
                    ("TagId", tagId),
                    ("UpdateRequest", request)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task DeleteUserTag(int tagId, int userId)
    {
        _logger.LogInformation("Deleting user tag [{TagId}] for user [{UserId}]", tagId, userId);
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

                                 DELETE FROM {UserTagsTable.SelectName}
                                 WHERE {UserTagIdField.SelectName} = @TagId
                                   AND {UserTagUserIdField.SelectName} = @UserId
                             
                 """;
            command.Parameters.Add(UserTagIdField.GenerateParameter("@TagId", tagId));
            command.Parameters.Add(UserTagUserIdField.GenerateParameter("@UserId", userId));

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected != 1)
            {
                throw new InvalidOperationException($"Deleted [{rowsAffected}] user tags when one user tag should have been deleted.");
            }

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Deleting User Tag",
                ex.Message,
                [
                    ("TagId", tagId),
                    ("UserId", userId)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<List<GroupTag>> GetGroupTags(int groupId)
    {
        _logger.LogInformation("Getting group tags for group [{GroupId}]", groupId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
                $"""

                                 {GroupTagsTable.GenerateSelect()}
                                 WHERE {GroupTagGroupIdField.SelectName} = @GroupId
                             
                 """;
            command.Parameters.Add(GroupTagGroupIdField.GenerateParameter("@GroupId", groupId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            List<GroupTag> tags = [];

            while (await reader.ReadAsync())
            {
                tags.Add(DtoReaders.ReadGroupTag(reader));
            }

            if (tags.Count is 0)
            {
                _logger.LogInformation("No group tags found for group [{GroupId}]", groupId);
            }

            return tags;
        }
        catch (Exception ex)
        {
            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting Group Tags",
                ex.Message,
                [
                    ("GroupId", groupId)
                ],
                command: command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<GroupTag> GetGroupTagById(int groupTagId)
    {
        _logger.LogInformation("Getting group tag [{GroupTagId}]", groupTagId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
                $"""

                                 {GroupTagsTable.GenerateSelect()}
                                 WHERE {GroupTagIdField.SelectName} = @GroupTagId
                             
                 """;
            command.Parameters.Add(GroupTagIdField.GenerateParameter("@GroupTagId", groupTagId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                GroupTag tag = DtoReaders.ReadGroupTag(reader);
                _logger.LogInformation("Found [{GroupTag}] for [{GroupTagId}]", tag, groupTagId);
                return tag;
            }

            _logger.LogInformation("No group tag found for [{GroupTagId}]", groupTagId);
            throw new InvalidOperationException($"No group tag found for GroupTagId [{groupTagId}].");
        }
        catch (Exception ex)
        {
            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting Group Tag",
                ex.Message,
                [
                    ("GroupTagId", groupTagId)
                ],
                command: command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<GroupTag> CreateGroupTag(TagCreateRequest request, int groupId)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        _logger.LogInformation("Creating group tag for group [{GroupId}]", groupId);
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

                                 INSERT INTO {GroupTagsTable.SelectName}
                                 ({GroupTagGroupIdField.SelectName},
                                  {GroupTagNameField.SelectName},
                                  {GroupTagColorField.SelectName})
                                 OUTPUT INSERTED.{GroupTagsTable.GetFields()}
                                 VALUES
                                 (@GroupId, @TagName, @TagColor)
                             
                 """;
            command.Parameters.Add(GroupTagGroupIdField.GenerateParameter("@GroupId", groupId));
            command.Parameters.Add(GroupTagNameField.GenerateParameter("@TagName", request.Name));
            command.Parameters.Add(GroupTagColorField.GenerateParameter("@TagColor", request.Color));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync() is false)
            {
                throw new InvalidOperationException("Failed to create group tag.");
            }

            GroupTag tag = DtoReaders.ReadGroupTag(reader);

            await transaction.CommitAsync();

            return tag;
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Creating Group Tag",
                ex.Message,
                [
                    ("GroupId", groupId),
                    ("CreateRequest", request)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<GroupTag?> UpdateGroupTag(TagUpdateRequest request, int tagId)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        _logger.LogInformation("Updating group tag [{TagId}]", tagId);
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

                                 UPDATE {GroupTagsTable.SelectName}
                                 SET {GroupTagNameField.SelectName} = @TagName,
                                     {GroupTagColorField.SelectName} = @TagColor
                                 OUTPUT INSERTED.{GroupTagsTable.GetFields()}
                                 WHERE {GroupTagIdField.SelectName} = @TagId
                             
                 """;
            command.Parameters.Add(GroupTagNameField.GenerateParameter("@TagName", request.Name));
            command.Parameters.Add(GroupTagColorField.GenerateParameter("@TagColor", request.Color));
            command.Parameters.Add(GroupTagIdField.GenerateParameter("@TagId", tagId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync() is false)
            {
                await transaction.CommitAsync();
                return null;
            }

            GroupTag tag = DtoReaders.ReadGroupTag(reader);

            if (await reader.ReadAsync())
            {
                throw new InvalidOperationException($"Updated more than one group tag for TagId [{tagId}].");
            }

            await transaction.CommitAsync();

            return tag;
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Updating Group Tag",
                ex.Message,
                [
                    ("TagId", tagId),
                    ("UpdateRequest", request)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task DeleteGroupTag(int tagId)
    {
        _logger.LogInformation("Deleting group tag [{TagId}]", tagId);
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

                                 DELETE FROM {GroupTagsTable.SelectName}
                                 WHERE {GroupTagIdField.SelectName} = @TagId
                             
                 """;
            command.Parameters.Add(GroupTagIdField.GenerateParameter("@TagId", tagId));

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected != 1)
            {
                throw new InvalidOperationException($"Deleted [{rowsAffected}] group tags when one group tag should have been deleted.");
            }

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Deleting Group Tag",
                ex.Message,
                [
                    ("TagId", tagId),
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }
}