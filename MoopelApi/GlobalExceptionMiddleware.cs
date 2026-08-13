using System.Text.Json;

using MoopelObjects.Exceptions;

namespace MoopelApi;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            string httpContextInfo = Helpers.FormatHelper.FormatHttpContext(context);
            _logger.LogError(ex, "Unhandled exception occurred. Context: {Context}", httpContextInfo);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        int statusCode = ex switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            ForbiddenException => StatusCodes.Status403Forbidden,
            ArgumentException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        var response = new
        {
            status = statusCode,
            title = statusCode switch
            {
                StatusCodes.Status404NotFound => "Not Found",
                StatusCodes.Status403Forbidden => "Forbidden",
                StatusCodes.Status400BadRequest => "Bad Request",
                StatusCodes.Status401Unauthorized => "Unauthorized",
                _ => "Server Error"
            },
            message = statusCode == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : ex.Message
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        string jsonResponse = JsonSerializer.Serialize(response);

        return context.Response.WriteAsync(jsonResponse);
    }
}
