using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;

namespace ApiTests.TestData;

internal static class StorageTestData
{
    public const int NonExistentStorageId = 9999;
    public const int NonExistentStorageItemId = 9998;

    public static readonly StorageCreateRequest ValidCreateRequest = new()
    {
        Name = "StorageName",
        Description = "Storage description"
    };

    public static readonly StorageUpdateRequest ValidUpdateRequest = new()
    {
        Name = "StorageNewName",
        Description = "Updated storage description"
    };

    public static TheoryData<StorageCreateRequest, StorageUpdateRequest> ValidStorageUpdateCreateRequests
    {
        get
        {
            TheoryData<StorageCreateRequest, StorageUpdateRequest> data = [];

            foreach (StorageCreateRequest createRequest in ValidStorageCreateRequestValues())
            {
                foreach (StorageUpdateRequest updateRequest in ValidStorageUpdateRequestValues())
                {
                    data.Add(createRequest, updateRequest);
                }
            }

            return data;
        }
    }

    public static TheoryData<StorageCreateRequest> ValidStorageCreateRequests
    {
        get
        {
            TheoryData<StorageCreateRequest> data = [.. ValidStorageCreateRequestValues()];

            return data;
        }
    }

    public static TheoryData<StorageUpdateRequest> ValidStorageUpdateRequests
    {
        get
        {
            TheoryData<StorageUpdateRequest> data = [.. ValidStorageUpdateRequestValues()];

            return data;
        }
    }

    public static IEnumerable<StorageCreateRequest> ValidStorageCreateRequestValues()
    {
        yield return new StorageCreateRequest { Name = "Pantry", Description = "Food pantry" };
        yield return new StorageCreateRequest { Name = "Garage", Description = "Tool and garden storage" };
        yield return new StorageCreateRequest { Name = "Basement", Description = "Seasonal items" };
        yield return new StorageCreateRequest { Name = "Attic", Description = "Old boxes and decor" };
        yield return new StorageCreateRequest { Name = "Closet", Description = "Clothing storage" };
        yield return new StorageCreateRequest { Name = "Empty Description", Description = "" };
        yield return new StorageCreateRequest { Name = "A", Description = "Min name" };
    }

    public static IEnumerable<StorageUpdateRequest> ValidStorageUpdateRequestValues()
    {
        yield return new StorageUpdateRequest { Name = "Pantry Updated", Description = "Expanded pantry contents" };
        yield return new StorageUpdateRequest { Name = "Garage Updated", Description = "Organized tools" };
        yield return new StorageUpdateRequest { Name = "Basement Updated", Description = "Moved boxes" };
        yield return new StorageUpdateRequest { Name = "Closet Updated", Description = "Seasonal rotation" };
        yield return new StorageUpdateRequest { Name = "Empty Description Update", Description = "" };
        yield return new StorageUpdateRequest { Name = "A", Description = "Min name update" };
    }

}
