using Microsoft.Data.SqlClient;

using MoopelApi.Helpers;
using MoopelApi.Models.Configuration;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Enums;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;
using MoopelApi.Services.Interfaces;

using static MoopelApi.ConstantValues.Database;

namespace MoopelApi.Services;

public class WorkItemService : IWorkItemService
{
    private readonly ILogger<WorkItemService> _logger;
    private readonly INotificationService _notificationService;
    private readonly DatabaseConnection _connection;

    public WorkItemService(ILogger<WorkItemService> logger, INotificationService notificationService, DatabaseConnection connection)
    {
        _logger = logger;
        _notificationService = notificationService;
        _connection = connection;
    }

    public async Task AddWorkItemLink(int childWorkItemId, int parentWorkItemId)
    {
        _logger.LogInformation("Adding work item link [{ChildId}] and [{ParentId}]", childWorkItemId, parentWorkItemId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
                $"""

                                 INSERT INTO {WorkItemLinksTable.SelectName}
                                 ({WorkItemParentIdField.SelectName}, {WorkItemChildIdField.SelectName})
                                 VALUES 
                                 (@ParentId, @ChildId)
                             
                 """;
            command.Parameters.Add(WorkItemParentIdField.GenerateParameter("@ParentId", parentWorkItemId));
            command.Parameters.Add(WorkItemChildIdField.GenerateParameter("@ChildId", childWorkItemId));

            int rowsAffected = await command.ExecuteNonQueryAsync();
            if (rowsAffected != 1)
                throw new Exception($"Rows affected[{rowsAffected}] did not match expected 1.");
        }
        catch (Exception ex)
        {

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Adding Work Item Link",
                ex.Message,
                [
                    ("ChildId", childWorkItemId),
                    ("ParentId", parentWorkItemId)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task RemoveWorkItemLink(int childWorkItemId, int parentWorkItemId)
    {
        _logger.LogInformation("Removing work item link [{ChildId}] and [{ParentId}]", childWorkItemId, parentWorkItemId);
        SqlCommand? command = null;
        SqlTransaction? transaction = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
                $"""

                                 DELETE FROM {WorkItemLinksTable.SelectName}
                                 WHERE {WorkItemParentIdField.SelectName} = @ParentId
                                 AND   {WorkItemChildIdField.SelectName} = @ChildId
                             
                 """;
            command.Parameters.Add(WorkItemParentIdField.GenerateParameter("@ParentId", parentWorkItemId));
            command.Parameters.Add(WorkItemChildIdField.GenerateParameter("@ChildId", childWorkItemId));

            int rowsAffected = await command.ExecuteNonQueryAsync();
            if (rowsAffected != 1)
                throw new Exception($"Rows affected[{rowsAffected}] did not match expected 1.");
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Deleting Work Item Link",
                ex.Message,
                [
                    ("ChildId", childWorkItemId),
                    ("ParentId", parentWorkItemId)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<WorkItem> CreateWorkItem(WorkItemCreateRequest request, int groupId)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        _logger.LogInformation("Creating work item for group [{Id}]", groupId);
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

                                 INSERT INTO {WorkItemsTable.SelectName}
                                 ({WorkItemGroupIdField.SelectName}, 
                                  {WorkItemNameField.SelectName}, 
                                  {WorkItemDescriptionField.SelectName}, 
                                  {WorkItemPointsField.SelectName},
                                  {WorkItemTypeField.SelectName})
                                 OUTPUT INSERTED.{WorkItemIdField.SelectName} 
                                 VALUES
                                 (@GroupId, @Name, @Description, @Points, @Type)
                             
                 """;
            command.Parameters.Add(WorkItemGroupIdField.GenerateParameter("@GroupId", groupId));
            command.Parameters.Add(WorkItemNameField.GenerateParameter("@Name", request.Name));
            command.Parameters.Add(WorkItemDescriptionField.GenerateParameter("@Description", request.Description));
            command.Parameters.Add(WorkItemPointsField.GenerateParameter("@Points", request.Points));
            command.Parameters.Add(WorkItemTypeField.GenerateParameter("@Type", request.Type));

            int workItemId = (int)(await command.ExecuteScalarAsync()
                ?? throw new InvalidOperationException("Failed to create workitem."));

            await transaction.CommitAsync();

            return new()
            {
                ItemId = workItemId,
                GroupId = groupId,

                Name = request.Name,
                Description = request.Description,
                Points = request.Points,
                Type = request.Type,

                CreatedAtUtc = DateTime.UtcNow,
            };
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Creating Work Item",
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

    public async Task DeleteWorkItem(int itemId)
    {
        _logger.LogInformation("Deleting work item [{Id}]", itemId);
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

                                 DELETE FROM {WorkItemsTable.SelectName}
                                 WHERE {WorkItemIdField.SelectName} = @ItemId
                             
                 """;
            command.Parameters.Add(WorkItemIdField.GenerateParameter("@ItemId", itemId));

            int rowsAffected = await command.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Deleting Work Item",
                ex.Message,
                [
                    ("ItemId", itemId),
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<List<WorkItem>?> GetChildrenWorkItems(int parentItemId, CancellationToken cancelToken)
    {
        _logger.LogInformation("Getting all children work items for [{ItemId}]", parentItemId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync(cancelToken);

            command = connection.CreateCommand();

            command.CommandText =
                $"""

                                 SELECT {WorkItemLinksTable.GetFields(WorkItemLinksTable.Alias)}, {WorkItemsTable.GetFields(WorkItemsTable.Alias)}
                                 FROM {WorkItemsTable.SelectName} {WorkItemsTable.Alias}
                                 JOIN {WorkItemLinksTable.SelectName} {WorkItemLinksTable.Alias} 
                                     ON {WorkItemsTable.Alias}.{WorkItemIdField.SelectName} = {WorkItemLinksTable.Alias}.{WorkItemChildIdField.SelectName}
                                 WHERE {WorkItemLinksTable.Alias}.{WorkItemParentIdField.SelectName} = @ParentId
                             
                 """;
            command.Parameters.Add(WorkItemParentIdField.GenerateParameter("@ParentId", parentItemId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync(cancelToken);

            List<WorkItem> workItems = [];

            while (await reader.ReadAsync(cancelToken))
            {
                workItems.Add(DtoReaders.ReadWorkItem(reader, WorkItemsTable.Alias));
            }

            _logger.LogInformation("Found [{Count}] workItems", workItems.Count);
            return workItems;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Request to get all children work items for Parent[{ParentItemId}] was canceled",
                parentItemId);
            return [];
        }
        catch (Exception ex)
        {

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting Children WorkItems",
                ex.Message,
                [
                    ("ParentItemId", parentItemId)
                ],
                command: command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<List<WorkItem>?> GetParentWorkItems(int childItemId, CancellationToken cancelToken)
    {
        _logger.LogInformation("Getting all parent work items for [{ItemId}]", childItemId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync(cancelToken);

            command = connection.CreateCommand();

            command.CommandText =
                $"""

                                 SELECT {WorkItemLinksTable.GetFields(WorkItemLinksTable.Alias)}, {WorkItemsTable.GetFields(WorkItemsTable.Alias)}
                                 FROM {WorkItemsTable.SelectName} {WorkItemsTable.Alias}
                                 JOIN {WorkItemLinksTable.SelectName} {WorkItemLinksTable.Alias} 
                                     ON {WorkItemsTable.Alias}.{WorkItemIdField.SelectName} = {WorkItemLinksTable.Alias}.{WorkItemParentIdField.SelectName}
                                 WHERE {WorkItemLinksTable.Alias}.{WorkItemChildIdField.SelectName} = @ChildId
                             
                 """;
            command.Parameters.Add(WorkItemChildIdField.GenerateParameter("@ChildId", childItemId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync(cancelToken);

            List<WorkItem> workItems = [];

            while (await reader.ReadAsync(cancelToken))
            {
                workItems.Add(DtoReaders.ReadWorkItem(reader, WorkItemsTable.Alias));
            }

            _logger.LogInformation("Found [{Count}] workItems", workItems.Count);
            return workItems;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Request to get all parent work items for Child[{ChildItemId}] was canceled",
                childItemId);
            return [];
        }
        catch (Exception ex)
        {

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting Parent WorkItems",
                ex.Message,
                [
                    ("ChildItemId", childItemId)
                ],
                command: command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<WorkItem?> GetWorkItem(int itemId)
    {
        _logger.LogInformation("Getting WorkItem [{ItemId}]", itemId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
                $"""

                                 SELECT {WorkItemsTable.GenerateSelect()}
                                 WHERE {WorkItemIdField.SelectName} = @ItemId
                             
                 """;
            command.Parameters.Add(WorkItemIdField.GenerateParameter("@ItemId", itemId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            List<WorkItem> workItems = [];

            if (await reader.ReadAsync())
            {
                WorkItem item = DtoReaders.ReadWorkItem(reader);
                _logger.LogInformation("Found [{Item}] for [{ItemId}]", item, itemId);
                return item;
            }
            else
            {
                _logger.LogInformation("No item found for [{ItemId}]", itemId);
                return null;
            }
        }
        catch (Exception ex)
        {

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting WorkItem",
                ex.Message,
                [
                    ("WorkItemId", itemId)
                ],
                command: command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<List<WorkItem>?> GetWorkItems(int groupId, CancellationToken cancelToken, WorkItemType? type = null)
    {
        _logger.LogInformation("Getting WorkItems for group [{groupId}]", groupId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync(cancelToken);

            command = connection.CreateCommand();

            command.CommandText =
                $"""

                                 SELECT {WorkItemsTable.GenerateSelect()}
                                 WHERE {WorkItemGroupIdField.SelectName} = @GroupId
                             
                 """;
            if (type is not null)
            {
                command.CommandText += $" AND {WorkItemTypeField.SelectName} = @Type";
                command.Parameters.Add(WorkItemTypeField.GenerateParameter("@Type", type.ToString()));
            }
            command.Parameters.Add(WorkItemIdField.GenerateParameter("@GroupId", groupId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync(cancelToken);

            List<WorkItem> workItems = [];

            while (await reader.ReadAsync(cancelToken))
            {
                WorkItem item = DtoReaders.ReadWorkItem(reader);
                workItems.Add(item);
            }

            _logger.LogInformation("Found [{Count}] work items for [{GroupId}]", workItems.Count, groupId);
            return workItems;
        }
        catch (Exception ex)
        {

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting WorkItem For Group",
                ex.Message,
                [
                    ("GroupId", groupId),
                    ("Type", type)
                ],
                command: command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<WorkItem> UpdateWorkItem(WorkItemUpdateRequest request, int groupId)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        _logger.LogInformation("Updating work item for group [{Id}]", groupId);
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

                                 INSERT INTO {WorkItemsTable.SelectName}
                                 ({WorkItemGroupIdField.SelectName}, 
                                  {WorkItemNameField.SelectName}, 
                                  {WorkItemDescriptionField.SelectName}, 
                                  {WorkItemPointsField.SelectName},
                                  {WorkItemTypeField.SelectName})
                                 OUTPUT INSERTED.{WorkItemIdField.SelectName} 
                                 VALUES
                                 (@GroupId, @Name, @Description, @Points, @Type)
                             
                 """;
            command.Parameters.Add(WorkItemGroupIdField.GenerateParameter("@GroupId", groupId));
            command.Parameters.Add(WorkItemNameField.GenerateParameter("@Name", request.Name));
            command.Parameters.Add(WorkItemDescriptionField.GenerateParameter("@Description", request.Description));
            command.Parameters.Add(WorkItemPointsField.GenerateParameter("@Points", request.Points));
            command.Parameters.Add(WorkItemTypeField.GenerateParameter("@Type", request.Type));

            int workItemId = (int)(await command.ExecuteScalarAsync()
                ?? throw new InvalidOperationException("Failed to create workitem."));

            await transaction.CommitAsync();

            return new()
            {
                ItemId = workItemId,
                GroupId = groupId,

                Name = request.Name,
                Description = request.Description,
                Points = request.Points,
                Type = request.Type,

                CreatedAtUtc = DateTime.UtcNow,
            };
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Creating Work Item",
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
}
