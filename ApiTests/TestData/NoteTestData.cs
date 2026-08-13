using MoopelObjects.Enums;
using MoopelObjects.Requests.Creation;
using MoopelObjects.Requests.Update;

namespace ApiTests.TestData;

internal static class NoteTestData
{
    public const int MissingNoteId = 999999;

    public static readonly NoteCreateRequest ValidCreateRequest = new()
    {
        Type = NoteType.Other,
        Title = "NoteTitle",
        Content = "Note content example"
    };

    public static readonly NoteUpdateRequest ValidUpdateRequest = new()
    {
        Type = NoteType.Other,
        Title = "UpdatedNoteTitle",
        Content = "Updated note content"
    };

    public static IEnumerable<NoteCreateRequest> ValidNoteCreateRequestValues()
    {
        yield return new NoteCreateRequest { Type = NoteType.Other, Title = "Shopping List", Content = "Milk, Eggs, Bread" };
        yield return new NoteCreateRequest { Type = NoteType.Fridge, Title = "Fridge Inventory", Content = "Tomatoes, Cucumbers" };
        yield return new NoteCreateRequest { Type = NoteType.Storage, Title = "Storage Note", Content = "Boxes in attic" };
        yield return new NoteCreateRequest { Type = NoteType.WorkItem, Title = "Work Task", Content = "Complete ticket #123" };
        yield return new NoteCreateRequest { Type = NoteType.Recipe, Title = "Pancake Recipe", Content = "Flour, Milk, Egg" };
        yield return new NoteCreateRequest { Type = NoteType.Other, Title = "Empty Content", Content = "" };
        yield return new NoteCreateRequest { Type = NoteType.Other, Title = "A", Content = "Min title" };
    }

    public static IEnumerable<NoteUpdateRequest> ValidNoteUpdateRequestValues()
    {
        yield return new NoteUpdateRequest { Type = NoteType.Other, Title = "Updated Shopping List", Content = "Milk, Eggs, Bread, Butter" };
        yield return new NoteUpdateRequest { Type = NoteType.Fridge, Title = "Updated Fridge Inventory", Content = "Tomatoes (2), Cucumbers (1)" };
        yield return new NoteUpdateRequest { Type = NoteType.Storage, Title = "Updated Storage Note", Content = "Moved boxes to garage" };
        yield return new NoteUpdateRequest { Type = NoteType.WorkItem, Title = "Updated Work Task", Content = "Follow up on ticket #123" };
        yield return new NoteUpdateRequest { Type = NoteType.Recipe, Title = "Updated Pancake Recipe", Content = "Add vanilla" };
        yield return new NoteUpdateRequest { Type = NoteType.Other, Title = "Empty Content Update", Content = "" };
        yield return new NoteUpdateRequest { Type = NoteType.Other, Title = "A", Content = "Min title update" };
    }

    public static TheoryData<NoteCreateRequest> ValidNoteCreateRequests
    {
        get
        {
            TheoryData<NoteCreateRequest> data = [];
            foreach (NoteCreateRequest r in ValidNoteCreateRequestValues())
            {
                data.Add(r);
            }

            return data;
        }
    }

    public static TheoryData<NoteUpdateRequest> ValidNoteUpdateRequests
    {
        get
        {
            TheoryData<NoteUpdateRequest> data = [];
            foreach (NoteUpdateRequest r in ValidNoteUpdateRequestValues())
            {
                data.Add(r);
            }

            return data;
        }
    }

    public static TheoryData<NoteCreateRequest, NoteUpdateRequest> ValidNoteUpdateCreateRequests
    {
        get
        {
            TheoryData<NoteCreateRequest, NoteUpdateRequest> data = [];

            foreach (NoteCreateRequest createRequest in ValidNoteCreateRequestValues())
            {
                foreach (NoteUpdateRequest updateRequest in ValidNoteUpdateRequestValues())
                {
                    data.Add(createRequest, updateRequest);
                }
            }

            return data;
        }
    }
}
