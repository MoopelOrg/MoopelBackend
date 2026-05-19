namespace MoopelApi.Models.Dto;

public record GroupUser
{
    public required int GroupId { get; init; }
    public required int UserId { get; init; }

    public required bool IsOwner { get; init; }
    public required bool IsAdmin { get; init; }

    public required bool CanReadWorkItems { get; init; }
    public required bool CanCreateWorkItems { get; init; }
    public required bool CanUpdateWorkItems { get; init; }
    public required bool CanDeleteWorkItems { get; init; }

    public required bool CanCreateTags { get; init; }
    public required bool CanUpdateTags { get; init; }
    public required bool CanDeleteTags { get; init; }
}
