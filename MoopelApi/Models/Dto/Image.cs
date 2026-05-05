namespace MoopelApi.Models.Dto;

public record Image
{
    public required int ImageId { get; init; }

    public required string Name { get; init;  }
    public required string ContentType { get; init; }

    public required byte[] Data { get; init; }
    public required byte[]? Thumbnail { get; init; }

    public required long SizeBytes { get; init; }

    public required int? Width { get; init; }
    public required int? Height { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}
