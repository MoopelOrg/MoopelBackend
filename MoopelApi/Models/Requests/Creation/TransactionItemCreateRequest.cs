namespace MoopelApi.Models.Requests.Creation;

public record TransactionItemCreateRequest
{
    public required string Name { get; init; }
    public required decimal Amount { get; init; }
}
