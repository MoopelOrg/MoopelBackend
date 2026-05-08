namespace MoopelApi.Models.Requests.Creation;

public record FridgeItemCreateRequest
{
    public required string Name { get; init; }
    public required int Quantity { get; init; }
}
