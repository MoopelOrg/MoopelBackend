namespace MoopelApi.Models.Dto;

public record BankAccount
{
    public required int AccountId { get; init; }
    public required int UserId { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required decimal Balance { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
}
