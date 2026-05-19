namespace MoopelApi.Models.Dto;

public record StorageItem
{
    public required int ItemId { get; init; }
    public required int StorageId { get; init; }

    public required string Name { get; init; }
    public string? Description { get; init; }
    public required int Quantity { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}
