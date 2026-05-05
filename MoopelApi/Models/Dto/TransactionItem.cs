namespace MoopelApi.Models.Dto;

public record TransactionItem
{
    public required int ItemId { get; init; }
    public required int TransactionId { get; init; }

    public required string Name { get; init; }
    public required decimal Amount { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
}
