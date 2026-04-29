namespace MoopelApi.Models.Dto;

public record Note
{
    public required int NoteId { get; init; }
    public required int UserId { get; init; }
}
