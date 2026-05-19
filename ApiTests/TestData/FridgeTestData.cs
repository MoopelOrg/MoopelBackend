using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;

namespace ApiTests.TestData;

internal static class FridgeTestData
{

    public static readonly FridgeCreateRequest ValidCreateRequest = new()
    {
        Name = "FridgeName",
        Description = "FridgeDescription",
    };

    public static readonly FridgeUpdateRequest ValidUpdateRequest = new()
    {
        Name = "FridgeNewName",
        Description = "FridgeNewDescription",
    };

    public static TheoryData<FridgeCreateRequest, FridgeUpdateRequest> ValidFridgeUpdateCreateRequests
    {
        get
        {
            TheoryData<FridgeCreateRequest, FridgeUpdateRequest> data = [];

            foreach (FridgeCreateRequest createRequest in ValidFridgeCreateRequestValues())
            {
                foreach (FridgeUpdateRequest updateRequest in ValidFridgeUpdateRequestValues())
                {
                    data.Add(createRequest, updateRequest);
                }
            }

            return data;
        }
    }

    public static TheoryData<FridgeCreateRequest> ValidFridgeCreateRequests
    {
        get
        {
            TheoryData<FridgeCreateRequest> data = [];

            foreach (FridgeCreateRequest request in ValidFridgeCreateRequestValues())
            {
                data.Add(request);
            }

            return data;
        }
    }

    public static TheoryData<FridgeUpdateRequest> ValidFridgeUpdateRequests
    {
        get
        {
            TheoryData<FridgeUpdateRequest> data = [];

            foreach (FridgeUpdateRequest request in ValidFridgeUpdateRequestValues())
            {
                data.Add(request);
            }

            return data;
        }
    }

    public static IEnumerable<FridgeUpdateRequest> ValidFridgeUpdateRequestValues()
    {
        yield return new FridgeUpdateRequest
        {
            Name = "Updated Kitchen Fridge",
            Description = "Updated kitchen fridge description",
        };

        yield return new FridgeUpdateRequest
        {
            Name = "Updated Garage Fridge",
            Description = "Updated garage fridge description",
        };

        yield return new FridgeUpdateRequest
        {
            Name = "A",
            Description = "Minimum length name",
        };

        yield return new FridgeUpdateRequest
        {
            Name = "Fridge With Empty Description",
            Description = "",
        };
    }

    public static IEnumerable<FridgeCreateRequest> ValidFridgeCreateRequestValues()
    {
        yield return new FridgeCreateRequest
        {
            Name = "Kitchen Fridge",
            Description = "Kitchen fridge description",
        };

        yield return new FridgeCreateRequest
        {
            Name = "Garage Fridge",
            Description = "Garage fridge description",
        };

        yield return new FridgeCreateRequest
        {
            Name = "Mini Fridge",
            Description = "Mini fridge description",
        };

        yield return new FridgeCreateRequest
        {
            Name = "A",
            Description = "Minimum length name",
        };

        yield return new FridgeCreateRequest
        {
            Name = "Fridge With Empty Description",
            Description = "",
        };
    }
}
