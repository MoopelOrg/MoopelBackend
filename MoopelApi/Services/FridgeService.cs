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
            @$"
                INSERT INTO {FridgesTable.SelectName}
                ({FridgeUserIdField.SelectName}, 
                 {FridgeNameField.SelectName}, 
                 {FridgeDescriptionField.SelectName}
                OUTPUT INSERTED.{FridgeIdField.SelectName} 
                VALUES
                (@UserId, @Name, @Description)
            ";
            command.Parameters.Add(FridgeUserIdField.GenerateParameter("@UserId", userId));
            command.Parameters.Add(FridgeNameField.GenerateParameter("@Name", request.Name));
            command.Parameters.Add(FridgeDescriptionField.GenerateParameter("@Description", request.Description));

            int fridgeId = (int)(await command.ExecuteScalarAsync()
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

    public async Task<FridgeItem> CreateFridgeItem(int fridgeId, FridgeItemCreateRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteFridge(int fridgeId)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteFridgeItem(int itemId)
    {
        throw new NotImplementedException();
    }

    public async Task<Fridge?> GetFridgeById(int fridgeId)
    {
        throw new NotImplementedException();
    }

    public async Task<FridgeItem?> GetFridgeItemById(int itemId)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Fridge>?> GetFridgesByUserId(int userId, CancellationToken cancelToken)
    {
        throw new NotImplementedException();
    }

    public async Task<Fridge> UpdateFridge(int fridgeId, FridgeUpdateRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<FridgeItem> UpdateFridgeItem(int itemId, FridgeItemUpdateRequest request)
    {
        throw new NotImplementedException();
    }
}
