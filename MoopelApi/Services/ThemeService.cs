using MoopelApi.Models.Configuration;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;
using MoopelApi.Services.Interfaces;

using static MoopelApi.ConstantValues.Database;

namespace MoopelApi.Services;

public class ThemeService : IThemeService
{
    private readonly ILogger<ThemeService> _logger;
    private readonly INotificationService _notificationService;
    private readonly DatabaseConnection _connection;

    public ThemeService(ILogger<ThemeService> logger, INotificationService notificationService, DatabaseConnection connection)
    {
        _logger = logger;
        _notificationService = notificationService;
        _connection = connection;
    }

    public Task<Theme> CreateTheme(ThemeCreateRequest request, User creatingUser)
    {
        throw new NotImplementedException();
    }

    public Task DeleteTheme(int themeId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Theme>> GetAllThemes(CancellationToken cancelToken)
    {
        throw new NotImplementedException();
    }

    public Task<Theme?> GetThemeById(int themeId)
    {
        throw new NotImplementedException();
    }

    public Task<Theme> UpdateTheme(int themeId, ThemeUpdateRequest request)
    {
        throw new NotImplementedException();
    }
}
