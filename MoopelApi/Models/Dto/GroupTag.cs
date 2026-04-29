namespace MoopelApi.Models.Dto;

public record GroupTag : Tag
{
    public required int GroupId { get; init; }
}
