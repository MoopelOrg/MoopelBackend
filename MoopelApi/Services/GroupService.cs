using Microsoft.Data.SqlClient;
using MoopelApi.Helpers;
using MoopelApi.Models;
using MoopelApi.Models.Configuration;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;
using MoopelApi.Services.Interfaces;

using static MoopelApi.ConstantValues.Database;

namespace MoopelApi.Services;

public class GroupService : IGroupService
{
    private readonly ILogger<GroupService> _logger;
    private readonly INotificationService _notificationService;
    private readonly DatabaseConnection _connection;

    public GroupService(ILogger<GroupService> logger, INotificationService notificationService, DatabaseConnection connection)
    {
        _logger = logger;
        _notificationService = notificationService;
        _connection = connection;
    }

    public async Task<Group> CreateGroup(User creator, GroupCreateRequest createRequest)
    {
        _logger.LogInformation("Creating group for [{User}]", creator);
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
                INSERT INTO {GroupsTable.SelectName}
                ({GroupNameField.SelectName}, 
                 {GroupDescriptionField.SelectName}
                OUTPUT INSERTED.{GroupIdField.SelectName} 
                VALUES
                (@Name, @Description)
            ";
            command.Parameters.Add(GroupNameField.GenerateParameter("@Name", createRequest.Name));
            command.Parameters.Add(GroupDescriptionField.GenerateParameter("@Description", createRequest.Description));

            int groupId = (int)(await command.ExecuteScalarAsync()
                ?? throw new InvalidOperationException("Failed to create group."));

            await transaction.CommitAsync();

            return new()
            {
                GroupId = groupId,
                Name = createRequest.Name,
                Description = createRequest.Description,
                CreatedAtUtc = DateTime.UtcNow,
            };
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Creating Group",
                ex.Message,
                [
                    ("Creator", creator),
                    ("CreateRequest", createRequest)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task DeleteGroup(int groupId)
    {
        _logger.LogInformation("Deleting group [{GroupId}]", groupId);
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
                DELETE FROM {GroupsTable.SelectName}
                WHERE {GroupIdField.SelectName} = @GroupId
            ";
            command.Parameters.Add(GroupIdField.GenerateParameter("@GroupId", groupId));

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected != 1)
                throw new InvalidOperationException($"Failed to delete group. Rows affected: {rowsAffected}");

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Deleting Group",
                ex.Message,
                [
                    ("GroupId", groupId),
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<Group?> GetGroup(int groupId)
    {
        _logger.LogInformation("Getting group [{GroupId}]", groupId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
            @$"
                {GroupsTable.GenerateSelect()}
                WHERE {GroupIdField.SelectName} = @GroupId
            ";
            command.Parameters.Add(GroupIdField.GenerateParameter("@GroupId", groupId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                _logger.LogInformation("Found [{GroupId}]", groupId);
                Group group = DtoReaders.ReadGroup(reader);
                return group;
            }
            else
            {
                _logger.LogInformation("Could not find [{GroupId}]", groupId);
                return null;
            }
        }
        catch (Exception ex)
        {
            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting Group",
                ex.Message,
                [
                    ("GroupId", groupId),
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<List<Group>?> GetUserGroups(int userId)
    {
        _logger.LogInformation("Getting groups for user [{UserId}]", userId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
            @$"
                {GroupsTable.GetFields(GroupsTable.Alias)}
                FROM {GroupsTable.SelectName} AS {GroupsTable.Alias}
                JOIN {GroupUsersTable.SelectName} AS {GroupUsersTable.Alias}
                    ON {GroupUsersTable.Alias}.{GroupUsersUserIdField.SelectName} = {GroupsTable.Alias}.{GroupIdField.SelectName}
                WHERE {GroupUsersTable.Alias}.{GroupUsersUserIdField.SelectName} = @UserId
            ";
            command.Parameters.Add(GroupUsersUserIdField.GenerateParameter("@UserId", userId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (reader.HasRows is false)
            {
                _logger.LogInformation("No groups found for user [{UserId}]", userId);
                return null;
            }

            List<Group> groups = [];
            while (await reader.ReadAsync())
            {
                groups.Add(DtoReaders.ReadGroup(reader, GroupsTable.Alias));
            }

            _logger.LogInformation("Found [{GroupCount}] groups for user [{UserId}]", groups.Count, userId);
            return groups;
        }
        catch (Exception ex)
        {
            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting Groups For User",
                ex.Message,
                [
                    ("UserId", userId),
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<List<User>?> GetUsersInGroup(int groupId)
    {
        _logger.LogInformation("Getting users in group [{GroupId}]", groupId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
            @$"
                {UsersTable.GetFields(UsersTable.Alias)}
                FROM {UsersTable.SelectName} AS {UsersTable.Alias}
                JOIN {GroupUsersTable.SelectName} AS {GroupUsersTable.Alias}
                    ON {GroupUsersTable.Alias}.{GroupUsersUserIdField.SelectName} = {UsersTable.Alias}.{UserIdField.SelectName}
                WHERE {GroupUsersTable.Alias}.{GroupUsersGroupIdField.SelectName} = @GroupId
            ";
            command.Parameters.Add(GroupUsersGroupIdField.GenerateParameter("@GroupId", groupId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (reader.HasRows is false)
            {
                _logger.LogInformation("No users found in group [{GroupId}]", groupId);
                return null;
            }

            List<User> users = [];
            while (await reader.ReadAsync())
            {
                users.Add(DtoReaders.ReadUser(reader, UsersTable.Alias));
            }

            _logger.LogInformation("Found [{UserCount}] users in group [{GroupId}]", users.Count, groupId);
            return users;
        }
        catch (Exception ex)
        {
            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting Users In Group",
                ex.Message,
                [
                    ("GroupId", groupId),
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<Group> UpdateGroup(int groupId, GroupUpdateRequest updateRequest)
    {
        _logger.LogInformation("Updating group [{GroupId}]", groupId);
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
                UPDATE {GroupsTable.SelectName}
                SET {GroupNameField.SelectName} = @Name,
                    {GroupDescriptionField.SelectName} = @Description
                OUTPUT INSERTED.*
                WHERE {GroupIdField.SelectName} = @GroupId
            ";
            command.Parameters.Add(GroupIdField.GenerateParameter("@GroupId", groupId));
            command.Parameters.Add(GroupNameField.GenerateParameter("@Name", updateRequest.Name));
            command.Parameters.Add(GroupDescriptionField.GenerateParameter("@Description", updateRequest.Description));

            List<Group> updatedGroups = [];

            using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                updatedGroups.Add(DtoReaders.ReadGroup(reader, GroupsTable.Alias));
            }

            if (updatedGroups.Count != 1)
                throw new InvalidOperationException($"Failed to update group. Groups updated: {updatedGroups.Count}");

            await transaction.CommitAsync();

            return updatedGroups[0];
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Updating Group",
                ex.Message,
                [
                    ("GroupId", groupId),
                    ("UpdateRequest", updateRequest)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<GroupUser> AddUserToGroup(int groupId, int userId)
    {
        _logger.LogInformation("Adding user [{UserId}] to group [{GroupId}]", userId, groupId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
            @$"
                INSERT INTO {GroupUsersTable.SelectName}
                ({GroupUsersGroupIdField.SelectName}, {GroupUsersUserIdField.SelectName})
                OUTPUT INSERTED.*
                VALUES (@GroupId, @UserId)
            ";
            command.Parameters.Add(GroupIdField.GenerateParameter("@GroupId", groupId));
            command.Parameters.Add(UserIdField.GenerateParameter("@UserId", userId));

            using SqlDataReader reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return DtoReaders.ReadGroupUser(reader, GroupUsersTable.Alias);
            }
            else
                throw new InvalidOperationException("Failed to add user to group.");
        }
        catch (Exception ex)
        {
            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Adding User to Group",
                ex.Message,
                [
                    ("GroupId", groupId),
                    ("UserId", userId)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<GroupUser> UpdateGroupUser(int groupId, int userId, GroupUserUpdateRequest updateRequest)
    {
        _logger.LogInformation("Updating group user access for user [{UserId}] in group [{GroupId}]", userId, groupId);
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
                UPDATE {GroupUsersTable.SelectName}
                SET {GroupUsersCanReadWorkItemsField.SelectName} = @CanViewWorkItems,
                    {GroupUsersCanCreateWorkItemsField.SelectName} = @CanCreateWorkItems,
                    --GroupUsersCanUpdateWorkItemsField.SelectName = @CanUpdateWorkItems,
                    --GroupUsersCanDeleteWorkItemsField.SelectName = @CanDeleteWorkItems
                OUTPUT INSERTED.*
                WHERE {GroupUsersGroupIdField.SelectName} = @GroupId 
                AND {GroupUsersUserIdField.SelectName} = @UserId
            ";
            command.Parameters.Add(GroupIdField.GenerateParameter("@GroupId", groupId));
            command.Parameters.Add(GroupUsersUserIdField.GenerateParameter("@UserId", userId));

            command.Parameters.Add(GroupUsersCanReadWorkItemsField.GenerateParameter("@CanViewWorkItems", updateRequest.CanViewWorkItems));
            command.Parameters.Add(GroupUsersCanCreateWorkItemsField.GenerateParameter("@CanCreateWorkItems", updateRequest.CanCreateWorkItems));
            //command.Parameters.Add(GroupUsersCanUpdateWorkItemsField.GenerateParameter("@CanUpdateWorkItems", updateRequest.CanUpdateWorkItems));
            //command.Parameters.Add(GroupUsersCanDeleteWorkItemsField.GenerateParameter("@CanDeleteWorkItems", updateRequest.CanDeleteWorkItems));

            List<GroupUser> updatedUsers = [];

            using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                updatedUsers.Add(DtoReaders.ReadGroupUser(reader));
            }

            if (updatedUsers.Count != 1)
                throw new InvalidOperationException($"Failed to update group user access. Users updated: {updatedUsers.Count}");

            await transaction.CommitAsync();

            return updatedUsers[0];
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Updating Group User Access",
                ex.Message,
                [
                    ("GroupId", groupId),
                    ("UserId", userId),
                    ("UpdateRequest", updateRequest)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }
}
