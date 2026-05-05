using MoopelApi.Models.Dto.Read;
using MoopelApi.Models.Enums;

namespace MoopelApi.Models.Dto;

public record User
{
    public required int UserId { get; init; }
    public required string Username { get; init; }
    public required string PasswordHash { get; set; }
    public string? Email { get; init; }
    public required UserRole Role { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? LastLoginUtc { get; init; }

    public required bool Deactivated { get; init; }

    public required int? ThemeId { get; init; }

    public UserRead ToRead()
    {
        return new UserRead()
        {
            UserId = UserId,
            Username = Username,
            Email = Email,
            Role = Role.ToString(),
            CreatedAtUtc = CreatedAtUtc,
            LastLoginUtc = LastLoginUtc,
            Deactivated = Deactivated
        };
    }
}
