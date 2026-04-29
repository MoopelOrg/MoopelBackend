namespace MoopelApi.Models.Dto;

public record UserTag : Tag
{
    public required int UserId { get; init; }
}
