using MoopelObjects.Dto;
using MoopelObjects.Requests.Creation;
using MoopelObjects.Requests.Update;

namespace MoopelApi.Services.Interfaces;

public interface IThemeService
{
    Task<List<Theme>> GetAllThemes(CancellationToken cancelToken);

    Task<Theme?> GetThemeById(int themeId);

    Task<Theme> CreateTheme(ThemeCreateRequest request, User creatingUser);

    Task<Theme> UpdateTheme(int themeId, ThemeUpdateRequest request);

    Task DeleteTheme(int themeId);
}
