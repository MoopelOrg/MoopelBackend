namespace MoopelApi.Models.Dto;

public record Fridge
{
    public required int UserId { get; init; }
    public required int FridgeId { get; init; }

    public required string Name { get; init; }
    public required string Description { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}
