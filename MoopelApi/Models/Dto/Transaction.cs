namespace MoopelApi.Models.Dto;

public record Transaction
{
    public required int TransactionId { get; init; }
    public required int AccountId { get; init; }

    public required List<TransactionItem> Items { get; init; }
}
