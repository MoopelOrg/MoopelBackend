namespace MoopelApi.Models.Dto;

public record Tag
{
    public required int TagId { get; init; }
    public required int GroupId { get; init; }
    public required string Name { get; init; }
    public required string Color { get; init; }
    public required bool ForNotes { get; init; }
    public required bool ForWorkItems { get; init; }
    public required bool ForTransactions { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}
