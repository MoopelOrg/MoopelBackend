namespace MoopelApi.Models.Requests.Update;

public record NoteUpdateRequest
{
    public required int NoteId { get; init; }   
}
