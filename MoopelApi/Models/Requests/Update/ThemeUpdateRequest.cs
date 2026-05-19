namespace MoopelApi.Models.Requests.Update;

public record ThemeUpdateRequest
{
    public required string Name { get; init; }
    public required string Description { get; init; }

    public required string Data { get; init; }
    public required bool IsDark { get; init; }
}
