namespace MoopelApi.Models.Dto;

public record Session
{
    public required int SessionId { get; init; }
    public required int UserId { get; init; }
}
