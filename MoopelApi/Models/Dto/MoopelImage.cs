namespace MoopelApi.Models.Dto;

public record MoopelImage
{
    public required int Id { get; init; }
    public MoopelFile? FullImage { get; init; } = null;
    public MoopelFile? Thumbnail { get; init; } = null;
}
