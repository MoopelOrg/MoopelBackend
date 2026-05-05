namespace MoopelApi.Models.Dto;

public record Theme
{
    public required int ThemeId { get; init; }

    public required string CreatorDescriptor { get; init; }

    public required string Name { get; init; }
    public required string Data { get; init; }
    public required string Description { get; init; }

    public required bool IsDark { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public required bool IsDeleted { get; init; }
}
