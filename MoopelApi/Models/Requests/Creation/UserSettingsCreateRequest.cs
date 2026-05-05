namespace MoopelApi.Models.Requests.Creation;

public record UserSettingsCreateRequest
{
    public string? Timezone { get; init; }
    public string? DateFormat { get; init; }
}
