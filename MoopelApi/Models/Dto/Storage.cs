namespace MoopelApi.Models.Dto;

public record Storage
{
    public required int StorageId { get; init; }
    public required int UserId { get; init; }

    public required string Name { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}
