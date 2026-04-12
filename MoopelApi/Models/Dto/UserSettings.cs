namespace MoopelApi.Models.Dto;

public class UserSettings
{
    public required int SettingsId { get; init; }
    public required int UserId { get; init; } 

    public required Theme ChosenTheme { get; init; }
}
