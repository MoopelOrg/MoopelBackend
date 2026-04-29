namespace MoopelApi.Models.Dto;

public record BankAccount
{
    public required int AccountId { get; init; }
    public required int UserId { get; init; }
}
