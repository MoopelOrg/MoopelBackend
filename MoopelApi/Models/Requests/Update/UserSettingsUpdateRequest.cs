namespace MoopelApi.Models.Requests.Update;

public record UserSettingsUpdateRequest
{
    public string? Timezone { get; init; }
    public string? DateFormat { get; init; }
}
