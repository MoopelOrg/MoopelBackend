namespace MoopelApi.Models.Requests.Creation;

public record BankAccountCreateRequest
{
    public required string Name { get; init; }
    public required string Description { get; init; }
}
