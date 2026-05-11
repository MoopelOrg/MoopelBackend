namespace MoopelApi.Models.Requests.Update;

public record StorageUpdateRequest
{
    public required string Name { get; init; }
    public required string Description { get; init; }
}
