using System.Threading.Tasks;
using Entities.Constants;
using Entities.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace API.Security;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string DefaultApiKey = "BTG_PROTOTYPING_SECRET_KEY_12345";

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant();

        // Bypass security for Swagger UI and Health Check endpoints
        if (path != null && (path.Contains("/swagger") || path.Contains("/health") || path == "/"))
        {
            await _next(context);
            return;
        }

        var configuredKey = configuration["Security:ApiKey"] ?? DefaultApiKey;

        if (!context.Request.Headers.TryGetValue(AppConstants.ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = Messages.InvalidApiKey });
            return;
        }

        if (!configuredKey.Equals(extractedApiKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = Messages.InvalidApiKey });
            return;
        }

        await _next(context);
    }
}
