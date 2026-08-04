namespace MoopelApi.Models.Requests.Update;

public record GroupUserUpdateRequest
{
    public required bool CanViewWorkItems { get; init; } = false;
    public required bool CanCreateWorkItems { get; init; } = false;
    public required bool CanUpdateWorkItems { get; init; } = false;
    public required bool CanDeleteWorkItems { get; init; } = false;

    public required bool CanCreateTags { get; init; } = false;
    public required bool CanUpdateTags { get; init; } = false;
    public required bool CanDeleteTags { get; init; } = false;
}
