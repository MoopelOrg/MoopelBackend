namespace MoopelApi.Models.Dto;

public record FridgeItem
{
    public required int FridgeItemId { get; init; }
    public required int FridgeId { get; init; }
}
