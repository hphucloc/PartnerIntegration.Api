using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

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
                    var problemDetails = new ProblemDetails
                    {
                        Title = "An unexpected error occurred.",
                        Status = StatusCodes.Status500InternalServerError,
                        Detail = exceptionHandlerPathFeature?.Error?.Message
                    };

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsJsonAsync(problemDetails);
                });
            });
        }
    }
}
