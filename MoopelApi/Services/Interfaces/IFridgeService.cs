using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;

namespace MoopelApi.Services.Interfaces;

public interface IFridgeService
{
    Task<List<Fridge>?> GetFridgesByUserId(int userId, CancellationToken cancelToken);

    Task<Fridge?> GetFridgeById(int fridgeId);

    Task<Fridge> CreateFridge(int userId, FridgeCreateRequest request);

    Task<Fridge> UpdateFridge(int fridgeId, FridgeUpdateRequest request);

    Task DeleteFridge(int fridgeId);

    Task<FridgeItem?> GetFridgeItemById(int itemId);

    Task<FridgeItem> CreateFridgeItem(int fridgeId, FridgeItemCreateRequest request);

    Task<FridgeItem> UpdateFridgeItem(int itemId, FridgeItemUpdateRequest request);

    Task DeleteFridgeItem(int itemId);
}
