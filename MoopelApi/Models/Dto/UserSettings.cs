namespace MoopelApi.Models.Dto;

public record UserSettings
{
    public required int SettingId { get; init; }
    public required int UserId { get; init; }

    public string? Timezone { get; init; }
    public string? DateFormat { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime LastUpdatedAtUtc { get; init; }
}
