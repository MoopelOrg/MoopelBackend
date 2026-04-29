using MoopelApi.Models.Dto;

namespace MoopelApi.Models;

public class GroupAccess
{
    public required User User { get; init; }
    public required Group Group { get; init; }

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
