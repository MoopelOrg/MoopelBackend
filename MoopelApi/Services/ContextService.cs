using MoopelApi.Models;
using MoopelApi.Models.Configuration;
using MoopelApi.Models.Dto;
using MoopelApi.Services.Interfaces;

namespace MoopelApi.Services;

public class ContextService : IContextService
{
    private readonly ILogger<ContextService> _logger;
    private readonly INotificationService _notificationService;
    private readonly DatabaseConnection _connection;

    public ContextService(ILogger<ContextService> logger, INotificationService notificationService, DatabaseConnection connection)
    {
        _logger = logger;
        _notificationService = notificationService;
        _connection = connection;
    }
    public Task<User> GetContextUser(HttpContext context)
    {
        throw new NotImplementedException();
    }

    public Task<RequestContext> GetRequestContext(HttpContext context)
    {
        throw new NotImplementedException();
    }
}
