namespace MoopelApi.Models.Requests.Update;

public record FridgeUpdateRequest
{
    public required string Name { get; init; }
    public required string Description { get; init; }
}
