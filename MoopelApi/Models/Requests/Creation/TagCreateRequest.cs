namespace MoopelApi.Models.Requests.Creation;

public record TagCreateRequest
{
    public required string Name { get; init; }
    public required string Color { get; init; }
}
