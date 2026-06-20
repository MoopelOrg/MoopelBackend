using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;

namespace ApiTests.TestData;

internal static class TagTestData
{
    public const int MissingTagId = 999999;
    public const int MissingGroupId = 999999;

    public static readonly TagCreateRequest ValidCreateRequest = new()
    {
        Name = "TagName",
        Color = "#1D4ED8"
    };

    public static readonly TagUpdateRequest ValidUpdateRequest = new()
    {
        Name = "UpdatedTagName",
        Color = "#EF4444"
    };

    public static IEnumerable<TagCreateRequest> ValidTagCreateRequestValues()
    {
        yield return new TagCreateRequest { Name = "Shopping", Color = "#16A34A" };
        yield return new TagCreateRequest { Name = "Work", Color = "#2563EB" };
        yield return new TagCreateRequest { Name = "Personal", Color = "#7C3AED" };
        yield return new TagCreateRequest { Name = "Urgent", Color = "#DC2626" };
        yield return new TagCreateRequest { Name = "A", Color = "#0F172A" };
    }

    public static IEnumerable<TagUpdateRequest> ValidTagUpdateRequestValues()
    {
        yield return new TagUpdateRequest { Name = "Updated Shopping", Color = "#22C55E" };
        yield return new TagUpdateRequest { Name = "Updated Work", Color = "#3B82F6" };
        yield return new TagUpdateRequest { Name = "Updated Personal", Color = "#A855F7" };
        yield return new TagUpdateRequest { Name = "Updated Urgent", Color = "#B91C1C" };
        yield return new TagUpdateRequest { Name = "A", Color = "#111827" };
    }

    public static TheoryData<TagCreateRequest> ValidTagCreateRequests
    {
        get
        {
            TheoryData<TagCreateRequest> data = [];

            foreach (TagCreateRequest request in ValidTagCreateRequestValues())
            {
                data.Add(request);
            }

            return data;
        }
    }

    public static TheoryData<TagUpdateRequest> ValidTagUpdateRequests
    {
        get
        {
            TheoryData<TagUpdateRequest> data = [];

            foreach (TagUpdateRequest request in ValidTagUpdateRequestValues())
            {
                data.Add(request);
            }

            return data;
        }
    }

    public static TheoryData<TagCreateRequest, TagUpdateRequest> ValidTagUpdateCreateRequests
    {
        get
        {
            TheoryData<TagCreateRequest, TagUpdateRequest> data = [];

            foreach (TagCreateRequest createRequest in ValidTagCreateRequestValues())
            {
                foreach (TagUpdateRequest updateRequest in ValidTagUpdateRequestValues())
                {
                    data.Add(createRequest, updateRequest);
                }
            }

            return data;
        }
    }
}
