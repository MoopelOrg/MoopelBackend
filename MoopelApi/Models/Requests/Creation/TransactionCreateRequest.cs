namespace MoopelApi.Models.Requests.Creation;

public record TransactionCreateRequest
{
    public required int AccountId { get; init; }
}
