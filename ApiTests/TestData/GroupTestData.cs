using MoopelObjects.Requests.Creation;
using MoopelObjects.Requests.Update;

namespace ApiTests.TestData;

internal static class GroupTestData
{
    public static readonly GroupCreateRequest ValidCreateRequest = new()
    {
        Name = "GroupName"
    };

    public static readonly GroupUpdateRequest ValidUpdateRequest = new()
    {
        Name = "UpdatedGroupName",
        Description = "Description",
    };

    public static TheoryData<GroupCreateRequest, GroupUpdateRequest> ValidGroupUpdateCreateRequests
    {
        get
        {
            TheoryData<GroupCreateRequest, GroupUpdateRequest> data = [];

            foreach (GroupCreateRequest createRequest in ValidGroupCreateRequestValues())
            {
                foreach (GroupUpdateRequest updateRequest in ValidGroupUpdateRequestValues())
                {
                    data.Add(createRequest, updateRequest);
                }
            }

            return data;
        }
    }

    public static TheoryData<GroupCreateRequest> ValidGroupCreateRequests
    {
        get
        {
            TheoryData<GroupCreateRequest> data = [];

            foreach (GroupCreateRequest request in ValidGroupCreateRequestValues())
            {
                data.Add(request);
            }

            return data;
        }
    }

    public static TheoryData<GroupUpdateRequest> ValidGroupUpdateRequests
    {
        get
        {
            TheoryData<GroupUpdateRequest> data = [];

            foreach (GroupUpdateRequest request in ValidGroupUpdateRequestValues())
            {
                data.Add(request);
            }

            return data;
        }
    }

    public static IEnumerable<GroupUpdateRequest> ValidGroupUpdateRequestValues()
    {
        yield return new GroupUpdateRequest
        {
            Name = "Updated Projects",
            Description = "Updated Project Description"
        };

        yield return new GroupUpdateRequest
        {
            Name = "Updated Chores",
            Description = "Chore Description"
        };

        yield return new GroupUpdateRequest
        {
            Name = "New Name",
            Description = "Update Description"
        };

        yield return new GroupUpdateRequest
        {
            Name = "A",
            Description = "A DESC"
        };
    }

    public static IEnumerable<GroupCreateRequest> ValidGroupCreateRequestValues()
    {
        yield return new GroupCreateRequest
        {
            Name = "Projects"
        };

        yield return new GroupCreateRequest
        {
            Name = "Chores"
        };

        yield return new GroupCreateRequest
        {
            Name = "Shopping"
        };

        yield return new GroupCreateRequest
        {
            Name = "Work"
        };

        yield return new GroupCreateRequest
        {
            Name = "A"
        };
    }
}
