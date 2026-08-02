using MoopelApi.Models.Enums;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;

namespace ApiTests.TestData;

internal static class WorkItemTestData
{
    public const int MissingGroupId = 999999;
    public const int MissingWorkItemId = 999999;
    public const int MissingParentWorkItemId = 999998;

    public const string GroupName = "WorkGroup";
    public const string CreateGroupName = "CreateWorkItemGroup";
    public const string ReadGroupName = "GetWorkItemGroup";
    public const string UpdateGroupName = "UpdateWorkItemGroup";
    public const string DeleteGroupName = "DeleteWorkItemGroup";
    public const string PrivateGroupName = "PrivateWorkGroup";
    public const string PrivateListGroupName = "PrivateGroupItems";
    public const string ParentGroupName = "ParentGroup";
    public const string RemoveParentGroupName = "RemoveParentGroup";
    public const string FilterGroupName = "FilterWorkItemsGroup";

    public const string WorkItemName = "BuySupplies";
    public const string MissingGroupWorkItemName = "MissingGroupWorkItem";
    public const string ExistingWorkItemName = "FinishProject";
    public const string OldWorkItemName = "OldTitle";
    public const string UpdatedWorkItemName = "NewTitle";
    public const string MissingWorkItemName = "MissingItem";
    public const string DeleteWorkItemName = "DeleteMe";
    public const string PrivateWorkItemName = "PrivateWorkItem";
    public const string ParentWorkItemName = "ParentItem";
    public const string ChildWorkItemName = "ChildItem";
    public const string RemoveParentWorkItemName = "Parent";
    public const string RemoveChildWorkItemName = "Child";
    public const string FilterTaskWorkItemName = "FilterTaskWorkItem";
    public const string FilterStoryWorkItemName = "FilterStoryWorkItem";
    public const string InvalidWorkItemType = "NotAWorkItemType";

    public const string Description = "Work item description";
    public const string UpdatedDescription = "Updated work item description";
    public const decimal Points = 1.0m;
    public const decimal UpdatedPoints = 2.0m;
    public const WorkItemType Type = WorkItemType.Task;
    public const WorkItemType UpdatedType = WorkItemType.Story;

    public static readonly WorkItemCreateRequest ValidCreateRequest = new()
    {
        Name = WorkItemName,
        Description = Description,
        Points = Points,
        Type = Type
    };

    public static readonly WorkItemCreateRequest MissingGroupCreateRequest = new()
    {
        Name = MissingGroupWorkItemName,
        Description = Description,
        Points = Points,
        Type = Type
    };

    public static readonly WorkItemUpdateRequest ValidUpdateRequest = new()
    {
        Name = UpdatedWorkItemName,
        Description = UpdatedDescription,
        Points = UpdatedPoints,
        Type = UpdatedType
    };

    public static readonly WorkItemUpdateRequest MissingWorkItemUpdateRequest = new()
    {
        Name = MissingWorkItemName,
        Description = UpdatedDescription,
        Points = UpdatedPoints,
        Type = UpdatedType
    };

    public static readonly WorkItemLinkRequest ValidLinkRequest = new()
    {
        ParentWorkItemId = 1,
        ChildWorkItemId = 2
    };

    public static readonly WorkItemLinkRequest MissingLinkRequest = new()
    {
        ParentWorkItemId = MissingParentWorkItemId,
        ChildWorkItemId = MissingWorkItemId
    };
}
