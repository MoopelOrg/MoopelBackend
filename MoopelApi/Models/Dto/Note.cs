using MoopelApi.Models.Enums;

namespace MoopelApi.Models.Dto;

public record Note
{
    public required int NoteId { get; init; }
    public required int UserId { get; init; }

    public required NoteType Type { get; init; }

    public required string Title { get; init; }
    public required string Content { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
}
