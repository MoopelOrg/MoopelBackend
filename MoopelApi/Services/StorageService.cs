using Microsoft.Data.SqlClient;
using MoopelApi.Helpers;
using MoopelApi.Models.Configuration;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;
using MoopelApi.Services.Interfaces;

using static MoopelApi.ConstantValues.Database;

namespace MoopelApi.Services;

public class StorageService : IStorageService
{
    private readonly ILogger<StorageService> _logger;
    private readonly INotificationService _notificationService;
    private readonly DatabaseConnection _connection;

    public StorageService(ILogger<StorageService> logger, INotificationService notificationService, DatabaseConnection connection)
    {
        _logger = logger;
        _notificationService = notificationService;
        _connection = connection;
    }

    public async Task<Storage> CreateStorage(int userId, StorageCreateRequest request)
    {
        _logger.LogInformation("Creating storage for user [{UserId}]", userId);
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
                INSERT INTO {StoragesTable.SelectName}
                ({StorageUserIdField.SelectName},
                 {StorageNameField.SelectName},
                 {StorageDescriptionField.SelectName})
                OUTPUT INSERTED.{StoragesTable.GetFields()}
                VALUES
                (@UserId, @Name, @Description)
            ";
            command.Parameters.Add(StorageUserIdField.GenerateParameter("@UserId", userId));
            command.Parameters.Add(StorageNameField.GenerateParameter("@Name", request.Name));
            command.Parameters.Add(StorageDescriptionField.GenerateParameter("@Description", request.Description));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync() is false)
            {
                throw new InvalidOperationException("Failed to create storage.");
            }

            Storage storage = DtoReaders.ReadStorage(reader);

            await transaction.CommitAsync();

            return storage;
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Creating Storage",
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

    public async Task<StorageItem> CreateStorageItem(int storageId, StorageCreateRequest request)
    {
        _logger.LogInformation("Creating storage item for storage [{StorageId}]", storageId);
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
                INSERT INTO {StorageItemsTable.SelectName}
                ({StorageItemStorageIdField.SelectName},
                 {StorageItemNameField.SelectName},
                 {StorageItemDescriptionField.SelectName})
                OUTPUT INSERTED.{StorageItemsTable.GetFields()}
                VALUES
                (@StorageId, @Name, @Description)
            ";
            command.Parameters.Add(StorageItemStorageIdField.GenerateParameter("@StorageId", storageId));
            command.Parameters.Add(StorageItemNameField.GenerateParameter("@Name", request.Name));
            command.Parameters.Add(StorageItemDescriptionField.GenerateParameter("@Description", request.Description));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync() is false)
            {
                throw new InvalidOperationException("Failed to create storage item.");
            }

            StorageItem item = DtoReaders.ReadStorageItem(reader);

            await transaction.CommitAsync();

            return item;
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Creating Storage Item",
                ex.Message,
                [
                    ("StorageId", storageId),
                    ("CreateRequest", request)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task DeleteStorage(int storageId)
    {
        _logger.LogInformation("Deleting storage [{StorageId}]", storageId);
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
                DELETE FROM {StoragesTable.SelectName}
                WHERE {StorageIdField.SelectName} = @StorageId
            ";
            command.Parameters.Add(StorageIdField.GenerateParameter("@StorageId", storageId));

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected > 1)
            {
                throw new InvalidOperationException($"Deleted [{rowsAffected}] storages when only one storage should have been deleted.");
            }

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Deleting Storage",
                ex.Message,
                [
                    ("StorageId", storageId)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task DeleteStorageItem(int storageItemId)
    {
        _logger.LogInformation("Deleting storage item [{StorageItemId}]", storageItemId);
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
                DELETE FROM {StorageItemsTable.SelectName}
                WHERE {StorageItemIdField.SelectName} = @StorageItemId
            ";
            command.Parameters.Add(StorageItemIdField.GenerateParameter("@StorageItemId", storageItemId));

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected > 1)
            {
                throw new InvalidOperationException($"Deleted [{rowsAffected}] storage items when only one storage item should have been deleted.");
            }

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Deleting Storage Item",
                ex.Message,
                [
                    ("StorageItemId", storageItemId)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<Storage?> GetStorageById(int storageId)
    {
        _logger.LogInformation("Getting storage [{StorageId}]", storageId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
            @$"
                SELECT {StoragesTable.GenerateSelect()}
                WHERE {StorageIdField.SelectName} = @StorageId
            ";
            command.Parameters.Add(StorageIdField.GenerateParameter("@StorageId", storageId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                Storage storage = DtoReaders.ReadStorage(reader);
                _logger.LogInformation("Found [{Storage}] for [{StorageId}]", storage, storageId);
                return storage;
            }

            _logger.LogInformation("No storage found for [{StorageId}]", storageId);
            return null;
        }
        catch (Exception ex)
        {
            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting Storage",
                ex.Message,
                [
                    ("StorageId", storageId)
                ],
                command: command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<StorageItem?> GetStorageItemById(int itemId)
    {
        _logger.LogInformation("Getting storage item [{ItemId}]", itemId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
            @$"
                SELECT {StorageItemsTable.GenerateSelect()}
                WHERE {StorageItemIdField.SelectName} = @ItemId
            ";
            command.Parameters.Add(StorageItemIdField.GenerateParameter("@ItemId", itemId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                StorageItem item = DtoReaders.ReadStorageItem(reader);
                _logger.LogInformation("Found [{StorageItem}] for [{ItemId}]", item, itemId);
                return item;
            }

            _logger.LogInformation("No storage item found for [{ItemId}]", itemId);
            return null;
        }
        catch (Exception ex)
        {
            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting Storage Item",
                ex.Message,
                [
                    ("ItemId", itemId)
                ],
                command: command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<List<Storage>?> GetStoragesByUserId(int userId, CancellationToken cancelToken)
    {
        _logger.LogInformation("Getting storages for user [{UserId}]", userId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync(cancelToken);

            command = connection.CreateCommand();

            command.CommandText =
            @$"
                SELECT {StoragesTable.GenerateSelect()}
                WHERE {StorageUserIdField.SelectName} = @UserId
            ";
            command.Parameters.Add(StorageUserIdField.GenerateParameter("@UserId", userId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync(cancelToken);

            if (reader.HasRows is false)
            {
                _logger.LogInformation("No storages found for user [{UserId}]", userId);
                return null;
            }

            List<Storage> storages = [];

            while (await reader.ReadAsync(cancelToken))
            {
                storages.Add(DtoReaders.ReadStorage(reader));
            }

            return storages;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Request to get storages for user [{UserId}] was canceled", userId);
            return [];
        }
        catch (Exception ex)
        {
            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting Storages By User",
                ex.Message,
                [
                    ("UserId", userId)
                ],
                command: command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<Storage> UpdateStorage(int storageId, StorageUpdateRequest request)
    {
        _logger.LogInformation("Updating storage [{StorageId}]", storageId);
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
                UPDATE {StoragesTable.SelectName}
                SET {StorageNameField.SelectName} = @Name,
                    {StorageDescriptionField.SelectName} = @Description
                OUTPUT INSERTED.{StoragesTable.GetFields()}
                WHERE {StorageIdField.SelectName} = @StorageId
            ";
            command.Parameters.Add(StorageNameField.GenerateParameter("@Name", request.Name));
            command.Parameters.Add(StorageDescriptionField.GenerateParameter("@Description", request.Description));
            command.Parameters.Add(StorageIdField.GenerateParameter("@StorageId", storageId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync() is false)
            {
                throw new InvalidOperationException($"No storage found for StorageId [{storageId}].");
            }

            Storage storage = DtoReaders.ReadStorage(reader);

            if (await reader.ReadAsync())
            {
                throw new InvalidOperationException($"Updated more than one storage for StorageId [{storageId}].");
            }

            await transaction.CommitAsync();

            return storage;
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Updating Storage",
                ex.Message,
                [
                    ("StorageId", storageId),
                    ("UpdateRequest", request)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<StorageItem> UpdateStorageItem(int itemId, StorageUpdateRequest request)
    {
        _logger.LogInformation("Updating storage item [{ItemId}]", itemId);
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
                UPDATE {StorageItemsTable.SelectName}
                SET {StorageItemNameField.SelectName} = @Name,
                    {StorageItemDescriptionField.SelectName} = @Description
                OUTPUT INSERTED.{StorageItemsTable.GetFields()}
                WHERE {StorageItemIdField.SelectName} = @ItemId
            ";
            command.Parameters.Add(StorageItemNameField.GenerateParameter("@Name", request.Name));
            command.Parameters.Add(StorageItemDescriptionField.GenerateParameter("@Description", request.Description));
            command.Parameters.Add(StorageItemIdField.GenerateParameter("@ItemId", itemId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync() is false)
            {
                throw new InvalidOperationException($"No storage item found for ItemId [{itemId}].");
            }

            StorageItem item = DtoReaders.ReadStorageItem(reader);

            if (await reader.ReadAsync())
            {
                throw new InvalidOperationException($"Updated more than one storage item for ItemId [{itemId}].");
            }

            await transaction.CommitAsync();

            return item;
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Updating Storage Item",
                ex.Message,
                [
                    ("ItemId", itemId),
                    ("UpdateRequest", request)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }
}