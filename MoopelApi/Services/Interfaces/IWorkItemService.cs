using MoopelApi.Models.Dto;
using MoopelApi.Models.Enums;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;

namespace MoopelApi.Services.Interfaces;

public interface IWorkItemService
{
    Task<List<WorkItem>?> GetWorkItems(int groupId, WorkItemType? type = null);
    Task<WorkItem?> GetWorkItem(int itemId);

    Task<WorkItem> CreateWorkItem(WorkItemCreateRequest request, int groupId);
    Task<WorkItem> UpdateWorkItem(WorkItemUpdateRequest request, int groupId);
    Task<bool> DeleteWorkItem(int itemId);

    Task<List<WorkItem>?> GetParentWorkItems(int itemId);
    Task<List<WorkItem>?> GetChildrenWorkItems(int itemId);

    Task<bool> AddWorkItemLink(int childWorkItemId, int parentWorkItemId);
    Task<bool> RemoveWorkItemLink(int childWorkItemId, int parentWorkItemId);
}
