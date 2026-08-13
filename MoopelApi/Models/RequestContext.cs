using MoopelObjects.Enums;

namespace MoopelApi.Models;

public record RequestContext
{
    public int? UserId { get; init; }
    public string? Username { get; init; }
    public string? Email { get; init; }

    public UserRole? Role { get; init; }

    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }

    public string? TraceId { get; init; }
    public string? RequestPath { get; init; }
    public string? RequestMethod { get; init; }

    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;
}
