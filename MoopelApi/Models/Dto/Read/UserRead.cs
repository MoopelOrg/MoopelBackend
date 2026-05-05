namespace MoopelApi.Models.Dto.Read;

public record UserRead
{
    public required int UserId { get; init; }
    public required string Username { get; init; }
    public string? Email { get; init; }
    public required string Role { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? LastLoginUtc { get; init; }

    public required bool Deactivated { get; init; }
}
