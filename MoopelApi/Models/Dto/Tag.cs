namespace MoopelApi.Models.Dto;

public record Tag
{
    public required int TagId { get; init; }
    public required string Name { get; init; }
}
