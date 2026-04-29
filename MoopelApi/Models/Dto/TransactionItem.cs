namespace MoopelApi.Models.Dto;

public record TransactionItem
{
    public required int ItemId { get; init; }
    public required int TransactionId { get; init; }
}
