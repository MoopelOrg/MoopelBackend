namespace MoopelApi.Models;

public class MoopelFile
{
    public required string ContentType { get; init; }
    public required byte[] Data { get; init; }
}
