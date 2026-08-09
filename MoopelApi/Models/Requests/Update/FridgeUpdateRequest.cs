namespace MoopelApi.Models.Requests.Update;

public record FridgeUpdateRequest
{
    public required string Name { get; set; }
    public required string Description { get; set; }
}
