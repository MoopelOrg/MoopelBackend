namespace MoopelApi.Models.Requests.Creation;

public class WorkItemLinkRequest
{
    public required int ChildWorkItemId { get; init; }
    public required int ParentWorkItemId { get; init; }
}
