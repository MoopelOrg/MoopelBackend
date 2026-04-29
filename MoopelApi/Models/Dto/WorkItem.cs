using MoopelApi.Models.Enums;

namespace MoopelApi.Models.Dto;

public record WorkItem
{
    public required int WorkItemId { get; init; }
    public required int GroupId { get; init; }

    public required WorkItemType Type { get; init; }
}
