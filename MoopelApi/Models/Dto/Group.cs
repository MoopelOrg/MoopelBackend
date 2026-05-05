namespace MoopelApi.Models.Dto;

public record Group
{
    public required int GroupId { get; init; }

    public required string Name { get; init; }
    public required string? Description { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}
