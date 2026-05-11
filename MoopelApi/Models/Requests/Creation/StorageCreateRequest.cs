namespace MoopelApi.Models.Requests.Creation;

public record StorageCreateRequest
{
    public required string Name { get; init; }
    public required string Description { get; init; }
}
