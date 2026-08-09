using Microsoft.Data.SqlClient;

using MoopelApi.Helpers;
using MoopelApi.Models.Configuration;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;
using MoopelApi.Services.Interfaces;

using static MoopelApi.ConstantValues.Database;

namespace MoopelApi.Services;

public class FridgeService : IFridgeService
{
    private readonly ILogger<FridgeService> _logger;
    private readonly INotificationService _notificationService;
    private readonly DatabaseConnection _connection;

    public FridgeService(ILogger<FridgeService> logger, INotificationService notificationService, DatabaseConnection connection)
    {
        _logger = logger;
        _notificationService = notificationService;
        _connection = connection;
    }

    public async Task<Fridge> CreateFridge(int userId, FridgeCreateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        _logger.LogInformation("Creating fridge for user [{UserId}]", userId);
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

                                 INSERT INTO {FridgesTable.SelectName}
                                 ({FridgeUserIdField.SelectName}, 
                                  {FridgeNameField.SelectName}, 
                                  {FridgeDescriptionField.SelectName})
                                 {FridgesTable.GenerateOutput()}
                                 VALUES
                                 (@UserId, @Name, @Description)
                             
                 """;
            command.Parameters.Add(FridgeUserIdField.GenerateParameter("@UserId", userId));
            command.Parameters.Add(FridgeNameField.GenerateParameter("@Name", request.Name));
            command.Parameters.Add(FridgeDescriptionField.GenerateParameter("@Description", request.Description));

            Fridge createdFridge;
            await using (SqlDataReader reader = await command.ExecuteReaderAsync())
            {
                if (!await reader.ReadAsync())
                {
                    throw new InvalidOperationException("Failed to read created fridge.");
                }

                createdFridge = DtoReaders.ReadFridge(reader);
            }

            await transaction.CommitAsync();

            return createdFridge;
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Creating Fridge",
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

    public async Task<FridgeItem> CreateFridgeItem(int fridgeId, FridgeItemCreateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        _logger.LogInformation("Creating fridge item under fridge [{FridgeId}]", fridgeId);
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

                                 INSERT INTO {FridgeItemsTable.SelectName}
                                 ({FridgeItemFridgeIdField.SelectName}, 
                                  {FridgeItemNameField.SelectName}, 
                                  {FridgeItemQuantityField.SelectName}
                                 OUTPUT INSERTED.{FridgeItemIdField.SelectName} 
                                 VALUES
                                 (@UserId, @Name, @Quantity)
                             
                 """;
            command.Parameters.Add(FridgeItemNameField.GenerateParameter("@Name", request.Name));
            command.Parameters.Add(FridgeItemQuantityField.GenerateParameter("@Quantity", request.Quantity));

            int fridgeItemId = (int)(await command.ExecuteScalarAsync()
                ?? throw new InvalidOperationException("Failed to create fridge."));

            await transaction.CommitAsync();

            return new()
            {
                FridgeId = fridgeId,
                FridgeItemId = fridgeItemId,
                Name = request.Name,
                Quantity = request.Quantity,
                CreatedAtUtc = DateTime.UtcNow,
            };
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Creating Fridge Item",
                ex.Message,
                [
                    ("FridgeId", fridgeId),
                    ("CreateRequest", request)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task DeleteFridge(int fridgeId)
    {
        _logger.LogInformation("Deleting fridge [{FridgeId}]", fridgeId);
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

                                 DELETE FROM {FridgesTable.SelectName}
                                 WHERE {FridgeIdField.SelectName} = @FridgeId
                             
                 """;
            command.Parameters.Add(FridgeIdField.GenerateParameter("@FridgeId", fridgeId));

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected != 1)
                throw new InvalidOperationException($"Failed to delete fridge. Rows affected: {rowsAffected}");

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Deleting Fridge",
                ex.Message,
                [
                    ("FridgeId", fridgeId),
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task DeleteFridgeItem(int itemId)
    {
        _logger.LogInformation("Deleting fridge item [{ItemId}]", itemId);
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

                                 DELETE FROM {FridgeItemsTable.SelectName}
                                 WHERE {FridgeItemIdField.SelectName} = @ItemId
                             
                 """;
            command.Parameters.Add(FridgeItemIdField.GenerateParameter("@ItemId", itemId));

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected != 1)
                throw new InvalidOperationException($"Failed to delete fridge item. Rows affected: {rowsAffected}");

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Deleting Fridge Item",
                ex.Message,
                [
                    ("Itemid", itemId),
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<Fridge?> GetFridgeById(int fridgeId)
    {
        _logger.LogInformation("Getting fridge [{FridgeId}]", fridgeId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
                $"""
                    {FridgesTable.GenerateSelect()}
                    WHERE {FridgeIdField.SelectName} = @FridgeId
                 """;
            command.Parameters.Add(FridgeIdField.GenerateParameter("@FridgeId", fridgeId));

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                Fridge fridge = DtoReaders.ReadFridge(reader);
                return fridge;
            }
            else
            {
                return null;
            }
        }
        catch (Exception ex)
        {

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting Fridge",
                ex.Message,
                [
                    ("FridgeId", fridgeId),
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<FridgeItem?> GetFridgeItemById(int itemId)
    {
        _logger.LogInformation("Getting fridge item [{ItemId}]", itemId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
                $"""
                    {FridgeItemsTable.GenerateSelect()}
                    WHERE {FridgeItemIdField.SelectName} = @ItemId
                 """;
            command.Parameters.Add(FridgeItemIdField.GenerateParameter("@ItemId", itemId));

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                FridgeItem fridgeItem = DtoReaders.ReadFridgeItem(reader);
                return fridgeItem;
            }
            else
            {
                return null;
            }
        }
        catch (Exception ex)
        {

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting Fridge Item",
                ex.Message,
                [
                    ("ItemId", itemId),
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<List<Fridge>?> GetFridgesByUserId(int userId, CancellationToken cancelToken)
    {
        _logger.LogInformation("Getting fridges for user [{UserId}]", userId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync(cancelToken);

            command = connection.CreateCommand();

            command.CommandText =
                $"""
                    {FridgeItemsTable.GenerateSelect()}
                    WHERE {FridgeUserIdField.SelectName} = @UserId
                 """;
            command.Parameters.Add(FridgeUserIdField.GenerateParameter("@UserId", userId));

            using SqlDataReader reader = await command.ExecuteReaderAsync(cancelToken);

            if (reader.HasRows is false)
            {
                return null;
            }

            List<Fridge> fridges = [];
            while (await reader.ReadAsync(cancelToken))
            {
                Fridge fridge = DtoReaders.ReadFridge(reader);
                fridges.Add(fridge);
            }

            return fridges;
        }
        catch (Exception ex)
        {

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting Fridges By User",
                ex.Message,
                [
                    ("UserId", userId),
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<Fridge> UpdateFridge(int fridgeId, FridgeUpdateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        _logger.LogInformation("Updating fridge [{FridgeId}]", fridgeId);
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

                                 UPDATE {FridgesTable.SelectName}
                                 SET {FridgeNameField.SelectName} = @Name, 
                                     {FridgeDescriptionField.SelectName} = @Description
                                 OUTPUT INSERTED.{FridgeUserIdField.SelectName}
                                 WHERE {FridgeIdField.SelectName} = @FridgeId;
                             
                 """;

            command.Parameters.Add(FridgeIdField.GenerateParameter("@FridgeId", fridgeId));
            command.Parameters.Add(FridgeNameField.GenerateParameter("@Name", request.Name));
            command.Parameters.Add(FridgeDescriptionField.GenerateParameter("@Description", request.Description));

            int userId = (int)(await command.ExecuteScalarAsync()
                ?? throw new InvalidOperationException("Failed to create fridge."));

            await transaction.CommitAsync();

            return new()
            {
                FridgeId = fridgeId,
                UserId = userId,
                Name = request.Name,
                Description = request.Description,
                CreatedAtUtc = DateTime.UtcNow,
            };
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Updating Fridge",
                ex.Message,
                [
                    ("FrideId", fridgeId),
                    ("UpdateRequest", request)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<FridgeItem> UpdateFridgeItem(int itemId, FridgeItemUpdateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        _logger.LogInformation("Updating fridge item [{ItemId}]", itemId);
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

                                 UPDATE {FridgeItemsTable.SelectName}
                                 SET {FridgeItemNameField.SelectName} = @Name, 
                                     {FridgeItemQuantityField.SelectName} = @Quantity
                                 OUTPUT INSERTED.{FridgeItemFridgeIdField.SelectName}
                                 WHERE {FridgeItemIdField.SelectName} = @ItemId;
                             
                 """;

            command.Parameters.Add(FridgeItemIdField.GenerateParameter("@ItemId", itemId));
            command.Parameters.Add(FridgeItemNameField.GenerateParameter("@Name", request.Name));
            command.Parameters.Add(FridgeItemQuantityField.GenerateParameter("@Quantity", request.Quantity));

            int fridgeId = (int)(await command.ExecuteScalarAsync()
                ?? throw new InvalidOperationException("Failed to update fridge item."));

            await transaction.CommitAsync();

            return new()
            {
                FridgeId = fridgeId,
                FridgeItemId = itemId,
                Name = request.Name,
                Quantity = request.Quantity,
                CreatedAtUtc = DateTime.UtcNow,
            };
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Updating Fridge Item",
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
