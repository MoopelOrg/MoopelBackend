using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;

namespace ApiTests.TestData;

internal static class ThemeTestData
{
    public const int MissingThemeId = 999999;

    public static readonly ThemeCreateRequest ValidCreateRequest = new()
    {
        Name = "ThemeName",
        Description = "Theme description",
        Data = "{\"background\":\"#ffffff\"}",
        IsDark = false
    };

    public static readonly ThemeUpdateRequest ValidUpdateRequest = new()
    {
        Name = "UpdatedThemeName",
        Description = "Updated theme description",
        Data = "{\"background\":\"#111827\"}",
        IsDark = true
    };

    public static IEnumerable<ThemeCreateRequest> ValidThemeCreateRequestValues()
    {
        yield return new ThemeCreateRequest
        {
            Name = "Light Theme",
            Description = "Clean light theme",
            Data = "{\"background\":\"#ffffff\",\"text\":\"#111827\"}",
            IsDark = false
        };

        yield return new ThemeCreateRequest
        {
            Name = "Dark Theme",
            Description = "High contrast dark theme",
            Data = "{\"background\":\"#111827\",\"text\":\"#f9fafb\"}",
            IsDark = true
        };

        yield return new ThemeCreateRequest
        {
            Name = "Blue Theme",
            Description = "Blue-accented theme",
            Data = "{\"background\":\"#eff6ff\",\"accent\":\"#2563eb\"}",
            IsDark = false
        };
    }

    public static IEnumerable<ThemeUpdateRequest> ValidThemeUpdateRequestValues()
    {
        yield return new ThemeUpdateRequest
        {
            Name = "Updated Light Theme",
            Description = "Updated clean light theme",
            Data = "{\"background\":\"#f8fafc\",\"text\":\"#0f172a\"}",
            IsDark = false
        };

        yield return new ThemeUpdateRequest
        {
            Name = "Updated Dark Theme",
            Description = "Updated high contrast dark theme",
            Data = "{\"background\":\"#0f172a\",\"text\":\"#f8fafc\"}",
            IsDark = true
        };

        yield return new ThemeUpdateRequest
        {
            Name = "Updated Blue Theme",
            Description = "Updated blue-accented theme",
            Data = "{\"background\":\"#dbeafe\",\"accent\":\"#1d4ed8\"}",
            IsDark = false
        };
    }

    public static TheoryData<ThemeCreateRequest> ValidThemeCreateRequests
    {
        get
        {
            TheoryData<ThemeCreateRequest> data = [];

            foreach (ThemeCreateRequest request in ValidThemeCreateRequestValues())
            {
                data.Add(request);
            }

            return data;
        }
    }

    public static TheoryData<ThemeUpdateRequest> ValidThemeUpdateRequests
    {
        get
        {
            TheoryData<ThemeUpdateRequest> data = [];

            foreach (ThemeUpdateRequest request in ValidThemeUpdateRequestValues())
            {
                data.Add(request);
            }

            return data;
        }
    }

    public static TheoryData<ThemeCreateRequest, ThemeUpdateRequest> ValidThemeUpdateCreateRequests
    {
        get
        {
            TheoryData<ThemeCreateRequest, ThemeUpdateRequest> data = [];

            foreach (ThemeCreateRequest createRequest in ValidThemeCreateRequestValues())
            {
                foreach (ThemeUpdateRequest updateRequest in ValidThemeUpdateRequestValues())
                {
                    data.Add(createRequest, updateRequest);
                }
            }

            return data;
        }
    }
}
