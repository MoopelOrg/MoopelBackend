using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;

namespace MoopelApi.Services.Interfaces;

public interface IStorageService
{
    Task<List<Storage>?> GetStoragesByUserId(int userId, CancellationToken cancelToken);

    Task<Storage?> GetStorageById(int storageId);

    Task<Storage> CreateStorage(int userId, StorageCreateRequest request);

    Task<Storage> UpdateStorage(int storageId, StorageUpdateRequest request);

    Task DeleteStorage(int storageId);


    Task<StorageItem?> GetStorageItemById(int itemId);

    Task<StorageItem> CreateStorageItem(int storageId, StorageCreateRequest request);

    Task<StorageItem> UpdateStorageItem(int itemId, StorageUpdateRequest request);

    Task DeleteStorageItem(int storageItemId);
}
