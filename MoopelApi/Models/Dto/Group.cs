namespace MoopelApi.Models.Dto;

public class Group
{
    public required int GroupId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
}
