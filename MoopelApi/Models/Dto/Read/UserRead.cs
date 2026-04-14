using MoopelApi.Models.Enums;

namespace MoopelApi.Models.Dto.Read;

public record UserRead
{
    public required int UserId { get; init; }
    public required string Username { get; init; }
    public string? Email { get; init; }
    public required UserRole Role { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? LastLoginAtUtc { get; init; }

    public required bool Deactivated { get; init; }

    public required IEnumerable<Group> Groups { get; init; }
}
