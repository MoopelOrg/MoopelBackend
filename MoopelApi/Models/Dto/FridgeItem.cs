namespace MoopelApi.Models.Dto;

public record FridgeItem
{
    public required int FridgeItemId { get; init; }
    public required int FridgeId { get; init; }

    public required string Name { get; init; }
    public required int Quantity { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}
