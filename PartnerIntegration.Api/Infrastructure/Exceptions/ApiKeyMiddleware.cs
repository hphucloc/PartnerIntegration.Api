using Microsoft.AspNetCore.Mvc;
using PartnerIntegration.Api.Infrastructure.Configurations;

namespace PartnerIntegration.Api.Infrastructure.Exceptions
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ApiSecurityOptions _options;

        public ApiKeyMiddleware(RequestDelegate next, ApiSecurityOptions options)
        {
            _next = next;
            _options = options;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(ApiSecurityOptions.ApiKeyHeaderName, out var providedApiKey)
                || !string.Equals(providedApiKey, _options.ApiKey, StringComparison.Ordinal))
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "Unauthorized request.",
                    Status = StatusCodes.Status401Unauthorized,
                    Detail = $"The '{ApiSecurityOptions.ApiKeyHeaderName}' header is missing or invalid.",
                    Instance = context.Request.Path
                };
                problemDetails.Extensions["traceId"] = context.TraceIdentifier;

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(problemDetails);
                return;
            }

            await _next(context);
        }
    }
}
