namespace MoopelApi.Models;

public class GroupAccess
{
    public required int UserId { get; init; }
    public required int GroupId { get; init; }

    public required bool CanViewWorkItems { get; init; } = false;
    public required bool CanCreateWorkItems { get; init; } = false;
    public required bool CanUpdateWorkItems { get; init; } = false;
    public required bool CanDeleteWorkItems { get; init; } = false;


    public bool CanDeleteGroup()
    {
        return false;
    }
    public bool CanViewGroup()
    {
        return false;
    }
}
