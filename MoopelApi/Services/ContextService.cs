using System.Security.Claims;

using MoopelApi.Models;
using MoopelApi.Models.Configuration;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Enums;
using MoopelApi.Services.Interfaces;

namespace MoopelApi.Services;

public class ContextService : IContextService
{
    private readonly ILogger<ContextService> _logger;
    private readonly INotificationService _notificationService;
    private readonly DatabaseConnection _connection;
    private readonly IUserService _userService;

    public ContextService(ILogger<ContextService> logger, INotificationService notificationService, DatabaseConnection connection, IUserService userService)
    {
        _logger = logger;
        _notificationService = notificationService;
        _connection = connection;
        _userService = userService;
    }
    public async Task<User> GetContextUser(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        string? username = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User?.Identity?.Name;

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new UnauthorizedAccessException("No authenticated user found in context");
        }

        User user = await _userService.GetUserFromUserName(username)
            ?? throw new InvalidOperationException($"User '{username}' not found in database");

        return user;
    }

    public Task<RequestContext> GetRequestContext(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        int? userId = null;

        bool gotId = int.TryParse(context.User?.FindFirstValue(ClaimTypes.NameIdentifier), out int retrievedId);

        if (gotId)
        {
            userId = retrievedId;
        }

        string? emailClaim = context.User?.FindFirstValue(ClaimTypes.Email);
        string? email = string.IsNullOrEmpty(emailClaim) ? null : emailClaim;

        Enum.TryParse(context.User?.FindFirstValue(ClaimTypes.Role), out UserRole parsedRole);
        UserRole? role = context.User?.FindFirstValue(ClaimTypes.Role) is not null ? parsedRole : null;

        RequestContext requestContext = new()
        {
            UserId = userId,
            Username = context.User?.FindFirstValue(ClaimTypes.Name) ?? "Anonymous",
            Email = email,
            Role = role,
            IpAddress = context.Connection?.RemoteIpAddress?.ToString(),
            UserAgent = context.Request?.Headers.UserAgent.ToString(),
            TraceId = context.TraceIdentifier,
            RequestPath = context.Request?.Path.Value,
            RequestMethod = context.Request?.Method
        };

        return Task.FromResult(requestContext);
    }
}
