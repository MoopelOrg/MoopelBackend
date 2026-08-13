using MoopelObjects.Requests.Update;

namespace ApiTests.TestData;

internal static class UserSettingsTestData
{
    public const int MissingSettingId = 999999;
    public const string InitialTimezone = "America/New_York";
    public const string InitialDateFormat = "MM/dd/yyyy";
    public const string UpdatedTimezone = "UTC";
    public const string UpdatedDateFormat = "yyyy-MM-dd";

    public static readonly UserSettingsUpdateRequest ValidUpdateRequest = new()
    {
        Timezone = UpdatedTimezone,
        DateFormat = UpdatedDateFormat
    };
}
