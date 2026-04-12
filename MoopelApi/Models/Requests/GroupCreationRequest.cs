namespace MoopelApi.Models.Requests;

public record GroupCreationRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}
