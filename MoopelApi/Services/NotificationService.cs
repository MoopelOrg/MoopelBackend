using MoopelApi.Models.Configuration;
using MoopelApi.Services.Interfaces;

namespace MoopelApi.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly DatabaseConnection _connection;

    public NotificationService(ILogger<NotificationService> logger, DatabaseConnection connection)
    {
        _logger = logger;
        _connection = connection;
    }
}
