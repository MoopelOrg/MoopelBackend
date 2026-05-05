namespace MoopelApi.Models.Dto.Read;

public record StorageRead : Storage
{
    public required List<StorageItem> Items { get; init; }
}
