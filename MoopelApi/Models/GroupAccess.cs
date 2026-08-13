using MoopelObjects.Dto;

namespace MoopelApi.Models;

public class GroupAccess
{
    public required int UserId { get; init; }
    public required int GroupId { get; init; }

    public required bool IsGroupMember { get; init; } = false;

    public required bool CanViewWorkItems { get; init; } = false;
    public required bool CanCreateWorkItems { get; init; } = false;
    public required bool CanUpdateWorkItems { get; init; } = false;
    public required bool CanDeleteWorkItems { get; init; } = false;

    public required bool CanCreateTags { get; init; } = false;
    public required bool CanUpdateTags { get; init; } = false;
    public required bool CanDeleteTags { get; init; } = false;

    public required bool IsGroupOwner { get; init; } = false;
    public required bool IsGroupAdmin { get; init; } = false;

    #region Access Methods

    #region General Group Permissions
    public bool CanViewGroup()
    {
        return true;
    }
    public bool CanViewGroupDetails()
    {
        return IsGroupMember;
    }
    #endregion

    #region Group Member Permissions
    public bool CanViewGroupMembers()
    {
        return IsGroupMember;
    }
    public bool CanAddGroupMember()
    {
        return IsGroupOwner;
    }
    public bool CanRemoveGroupMember()
    {
        return IsGroupOwner;
    }
    public bool CanViewGroupSettings()
    {
        return IsGroupOwner;
    }
    public bool CanUpdateGroupSettings()
    {
        return IsGroupOwner;
    }
    public bool CanDeleteGroup()
    {
        return IsGroupOwner;
    }
    public bool CanUpdateGroup()
    {
        return IsGroupOwner;
    }
    #endregion

    #region Work Item Permissions
    public bool CanViewWorkItem()
    {
        return CanViewWorkItems;
    }
    public bool CanCreateWorkItem()
    {
        return CanCreateWorkItems;
    }
    public bool CanUpdateWorkItem()
    {
        return CanUpdateWorkItems;
    }
    public bool CanDeleteWorkItem()
    {
        return CanDeleteWorkItems;
    }
    #endregion

    #region Tag Permissions
    public bool CanCreateTag()
    {
        return CanCreateTags;
    }
    public bool CanUpdateTag()
    {
        return CanUpdateTags;
    }
    public bool CanDeleteTag()
    {
        return CanDeleteTags;
    }
    #endregion

    #endregion

    public static GroupAccess GetAccessFromDto(GroupUser groupUser)
    {
        ArgumentNullException.ThrowIfNull(groupUser, nameof(groupUser));

        return new()
        {
            UserId = groupUser.UserId,
            GroupId = groupUser.GroupId,

            IsGroupMember = true,

            IsGroupOwner = groupUser.IsOwner,
            IsGroupAdmin = groupUser.IsAdmin,

            CanViewWorkItems = groupUser.CanReadWorkItems,
            CanCreateWorkItems = groupUser.CanCreateWorkItems,
            CanUpdateWorkItems = groupUser.CanUpdateWorkItems,
            CanDeleteWorkItems = groupUser.CanDeleteWorkItems,

            CanCreateTags = groupUser.CanCreateTags,
            CanUpdateTags = groupUser.CanUpdateTags,
            CanDeleteTags = groupUser.CanDeleteTags,
        };
    }

    public static GroupAccess GetNoAccess(int userId, int groupId)
    {
        return new()
        {
            UserId = userId,
            GroupId = groupId,
            IsGroupMember = false,
            IsGroupOwner = false,
            IsGroupAdmin = false,
            CanViewWorkItems = false,
            CanCreateWorkItems = false,
            CanUpdateWorkItems = false,
            CanDeleteWorkItems = false,
            CanCreateTags = false,
            CanUpdateTags = false,
            CanDeleteTags = false,
        };
    }
}
