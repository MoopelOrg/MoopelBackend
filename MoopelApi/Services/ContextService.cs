using System.Net;
using System.Security.Claims;

using MoopelApi.Models;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Enums;
using MoopelApi.Services.Interfaces;

namespace MoopelApi.Services;

public class ContextService : IContextService
{
    private readonly ILogger<ContextService> _logger;
    private readonly INotificationService _notificationService;
    private readonly IUserService _userService;

    public ContextService(ILogger<ContextService> logger, INotificationService notificationService, IUserService userService)
    {
        _logger = logger;
        _notificationService = notificationService;
        _userService = userService;
    }
    public async Task<User> GetContextUser(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        string? userIdStr = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        string? userName = context.User?.FindFirstValue(ClaimTypes.Name);

        if (!int.TryParse(userIdStr, out int userId) || string.IsNullOrEmpty(userName))
        {
            throw new UnauthorizedAccessException("No authenticated user found in context");
        }

        User user = await _userService.GetUserFromUserId(userId)
            ?? throw new InvalidOperationException($"User '{userName} - {userId}' not found in database");

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

        bool roleParseResult = Enum.TryParse(context.User?.FindFirstValue(ClaimTypes.Role), out UserRole r);
        UserRole? parsedRole = r;

        if (roleParseResult is false)
        {
            parsedRole = null;
        }

        string? ipAddress = context.Connection?.RemoteIpAddress?.ToString();

        if (string.IsNullOrWhiteSpace(ipAddress) && context.Request.Host.Host == "localhost")
        {
            ipAddress = IPAddress.Loopback.ToString();
        }

        RequestContext requestContext = new()
        {
            UserId = userId,
            Username = context.User?.FindFirstValue(ClaimTypes.Name) ?? "Anonymous",
            Email = email,
            Role = parsedRole,
            IpAddress = ipAddress,
            UserAgent = context.Request?.Headers.UserAgent.ToString(),
            TraceId = context.TraceIdentifier,
            RequestPath = context.Request?.Path.Value,
            RequestMethod = context.Request?.Method
        };

        return Task.FromResult(requestContext);
    }
}
