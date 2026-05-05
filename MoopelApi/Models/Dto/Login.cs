using MoopelApi.Models.Enums;

namespace MoopelApi.Models.Dto;

public record Login
{
    public required int LoginId { get; init; }

    public required string Username { get; init; }
    public required DateTime AttemptTimeUtc { get; init; }

    public required LoginStatus Status { get; init; }
}
