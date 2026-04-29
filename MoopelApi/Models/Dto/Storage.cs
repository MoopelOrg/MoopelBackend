namespace MoopelApi.Models.Dto;

public record Storage
{
    public required int StorageId { get; init; }
    public required int UserId { get; init; }

    public required List<StorageItem> Items { get; init; }
}
