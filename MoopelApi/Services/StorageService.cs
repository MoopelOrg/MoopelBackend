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

    public Task<Storage> CreateStorage(int userId, StorageCreateRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<StorageItem> CreateStorageItem(int storageId, StorageCreateRequest request)
    {
        throw new NotImplementedException();
    }

    public Task DeleteStorage(int storageId)
    {
        throw new NotImplementedException();
    }

    public Task DeleteStorageItem(int storageItemId)
    {
        throw new NotImplementedException();
    }

    public Task<Storage?> GetStorageById(int storageId)
    {
        throw new NotImplementedException();
    }

    public Task<StorageItem?> GetStorageItemById(int itemId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Storage>?> GetStoragesByUserId(int userId, CancellationToken cancelToken)
    {
        throw new NotImplementedException();
    }

    public Task<Storage> UpdateStorage(int storageId, StorageUpdateRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<StorageItem> UpdateStorageItem(int itemId, StorageUpdateRequest request)
    {
        throw new NotImplementedException();
    }
}
