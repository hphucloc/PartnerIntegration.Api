using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client.Exceptions;

namespace PartnerIntegration.Api.Infrastructure.Exceptions
{
    public static class GlobalExceptionHandler
    {
        public static void ConfigureExceptionHandler(WebApplication app)
        {
            app.UseExceptionHandler(exceptionHandlerApp =>
            {
                exceptionHandlerApp.Run(async context =>
                {
                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    var exception = exceptionHandlerPathFeature?.Error;
                    var (statusCode, title) = MapException(exception);
                    var problemDetails = new ProblemDetails
                    {
                        Title = title,
                        Status = statusCode,
                        Detail = exception?.Message,
                        Instance = context.Request.Path
                    };
                    problemDetails.Extensions["traceId"] = context.TraceIdentifier;

                    context.Response.StatusCode = statusCode;
                    context.Response.ContentType = "application/problem+json";
                    await context.Response.WriteAsJsonAsync(problemDetails);
                });
            });
        }

        private static (int StatusCode, string Title) MapException(Exception? exception)
        {
            return exception switch
            {
                TimeoutException => (
                    StatusCodes.Status504GatewayTimeout,
                    "The request timed out."),
                HttpRequestException => (
                    StatusCodes.Status503ServiceUnavailable,
                    "A dependent service is unavailable."),
                BrokerUnreachableException => (
                    StatusCodes.Status503ServiceUnavailable,
                    "The message broker is unavailable."),
                _ => (
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.")
            };
        }
    }
}
