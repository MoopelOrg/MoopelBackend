namespace MoopelApi.Models.Requests.Update;

public record BankAccountUpdateRequest
{
    public required string Name { get; init; }
}
