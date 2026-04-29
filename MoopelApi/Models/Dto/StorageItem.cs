namespace MoopelApi.Models.Dto;

public record StorageItem
{
    public required int ItemId { get; init; }
    public required int StorageId { get; init; }
}
