namespace MoopelApi.Models.Requests.Update;

public record FridgeItemUpdateRequest
{
    public required string Name { get; init; }
    public required int Quantity { get; init; }
}
