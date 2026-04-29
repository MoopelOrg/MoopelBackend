namespace MoopelApi.Models.Requests.Creation;

public record GroupCreateRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}
