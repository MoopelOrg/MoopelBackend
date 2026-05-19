namespace MoopelApi.Models.Requests.Update;

public record BankAccountUpdateRequest
{
    public required string Name { get; init; }
    public required string Description { get; init; }
}
