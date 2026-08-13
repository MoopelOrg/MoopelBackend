using MoopelObjects.Dto;
using MoopelObjects.Enums;
using MoopelObjects.Requests.Creation;
using MoopelObjects.Requests.Update;

namespace MoopelApi.Services.Interfaces;

public interface IWorkItemService
{
    Task<List<WorkItem>?> GetWorkItems(int groupId, CancellationToken cancelToken, WorkItemType? type = null);
    Task<WorkItem?> GetWorkItem(int itemId);

    Task<WorkItem> CreateWorkItem(WorkItemCreateRequest request, int groupId);
    Task<WorkItem> UpdateWorkItem(WorkItemUpdateRequest request, int itemId);
    Task DeleteWorkItem(int itemId);

    Task<List<WorkItem>?> GetParentWorkItems(int itemId, CancellationToken cancelToken);
    Task<List<WorkItem>?> GetChildrenWorkItems(int itemId, CancellationToken cancelToken);

    Task AddWorkItemLink(int childWorkItemId, int parentWorkItemId);
    Task RemoveWorkItemLink(int childWorkItemId, int parentWorkItemId);
}
