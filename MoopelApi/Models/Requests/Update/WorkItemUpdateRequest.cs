namespace MoopelApi.Models.Requests.Update;

public record WorkItemUpdateRequest
{
    public required int WorkItemId { get; init; }
}
