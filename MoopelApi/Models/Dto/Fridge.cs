namespace MoopelApi.Models.Dto;

public record Fridge
{
    public required int UserId { get; init; }
    public required int FridgeId { get; init; }
}
