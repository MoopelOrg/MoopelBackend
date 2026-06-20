using MoopelApi.Models.Enums;

namespace MoopelApi.Models.Requests.Creation;

public record NoteCreateRequest
{
    public required NoteType Type { get; init; }
    public required string Title { get; init; }
    public required string Content { get; init; }
}
