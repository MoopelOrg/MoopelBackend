namespace MoopelApi.Models.Dto;

public record Transaction
{
    public required int TransactionId { get; init; }
    public required int AccountId { get; init; }

    public required decimal Amount { get; init; }
    public required string Description { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
}
