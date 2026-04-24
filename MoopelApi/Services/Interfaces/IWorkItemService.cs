using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests;

namespace MoopelApi.Services.Interfaces;

public interface IWorkItemService
{
    Task<List<WorkItem?>> GetWorkItemsForUser(int userId);
    Task<WorkItem?> GetWorkItemById(int itemId);
    
    Task<List<WorkItem>?> GetParentWorkItems(int itemId);
    Task<List<WorkItem>?> GetChildrenWorkItems(int itemId);

    Task<WorkItem?> CreateWorkItem(WorkItemCreationRequest request, int userId);
    Task<bool> DeleteWorkItem(int itemId);

    Task<bool> AddChildWorkItem(WorkItem workItem, int parentId);
    Task<bool> AddParentWorkItem(WorkItem workItem, int childId);
}
