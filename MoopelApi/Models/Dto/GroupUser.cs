namespace MoopelApi.Models.Dto;

public record GroupUser
{
    public required int GroupId { get; init; }
    public required int UserId { get; init; }


    public required bool CanReadWorkItems { get; init; }
    public required bool CanCreateWorkItems { get; init; }
}
