namespace MoopelApi.Models.Requests.Update;

public record TransactionItemUpdateRequest
{
    public required string Name { get; init; }
    public required decimal Amount { get; init; }
}
