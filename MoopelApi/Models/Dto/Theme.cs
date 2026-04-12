using System.Text.Json;

namespace MoopelApi.Models.Dto;

public record Theme
{
    public required int ThemeId { get; init; }
    public required int UserId { get; init; }

    public required int Name { get; init; }
    public required JsonElement ThemeSettings { get; init; } 
    public string? Description { get; init; }
    public required bool IsDark { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
}
