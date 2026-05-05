namespace MoopelApi.Models.Dto;

public record Session
{
    public required int SessionId { get; init; }
    public required int UserId { get; init; }

    public required int LoginId { get; init; }
    public required string HashedToken { get; init; }
    public required string IpIssuedTo { get; init; }
    public required DateTime IssuedAtUtc { get; init; }
    public required DateTime ExpiresAtUtc { get; init; }
    public required bool Revoked { get; init; }
}
