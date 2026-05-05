namespace MoopelApi.Models.Dto.Read;

public record FridgeRead : Fridge
{
    public required List<FridgeItem> Items { get; init; }
}
