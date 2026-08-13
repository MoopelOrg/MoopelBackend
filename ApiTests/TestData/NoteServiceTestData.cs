using MoopelObjects.Enums;
using MoopelObjects.Requests.Creation;
using MoopelObjects.Requests.Update;

namespace ApiTests.TestData;

internal static class NoteServiceTestData
{
    public const string PrimaryUsername = "noteservice-user";
    public const string PrimaryPassword = "NoteService1!";
    public const string PrimaryTitle = "Note service note";
    public const string PrimaryContent = "Note service content";
    public const string UpdatedTitle = "Updated note service note";
    public const string UpdatedContent = "Updated note service content";

    public const int MissingNoteId = 999995;
    public const int MissingUserId = 999994;

    public static readonly NoteCreateRequest ValidCreateRequest = new()
    {
        Type = NoteType.Other,
        Title = PrimaryTitle,
        Content = PrimaryContent
    };

    public static readonly NoteUpdateRequest ValidUpdateRequest = new()
    {
        Type = NoteType.Recipe,
        Title = UpdatedTitle,
        Content = UpdatedContent
    };
}
