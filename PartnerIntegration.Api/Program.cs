using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.OpenApi.Models;
using PartnerIntegration.Api.Infrastructure.Configurations;
using PartnerIntegration.Api.Infrastructure.Exceptions;
using PartnerIntegration.Api.Messaging;
using PartnerIntegration.Api.Services.Implementations;
using PartnerIntegration.Api.Services.Interfaces;
using Polly;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
var partnerVerificationBaseUrl =
    builder.Configuration[
        builder.Environment.IsDevelopment()
            ? "PartnerVerification:DevelopmentBaseUrl"
            : "PartnerVerification:BaseUrl"]
    ?? throw new InvalidOperationException("PartnerVerification:BaseUrl is not configured.");
var apiSecurityOptions = builder.Configuration
    .GetSection(ApiSecurityOptions.SectionName)
    .Get<ApiSecurityOptions>()
    ?? throw new InvalidOperationException($"{ApiSecurityOptions.SectionName} is not configured.");

// Add services to the container.
builder.Services.AddSingleton(apiSecurityOptions);
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RabbitMqOptions>>().Value);
builder.Services.AddSingleton<IRabbitMqClient, RabbitMqClient>();
builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
builder.Services.AddScoped<IPartnerVerificationService, PartnerVerificationService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        var problemDetails = new ProblemDetails
        {
            Title = "Too many requests.",
            Status = StatusCodes.Status429TooManyRequests,
            Detail = "Rate limit exceeded. Please retry later.",
            Instance = context.HttpContext.Request.Path
        };
        problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        context.HttpContext.Response.ContentType = "application/problem+json";
        await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
    };
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        if (!httpContext.Request.Path.StartsWithSegments("/api"))
        {
            return RateLimitPartition.GetNoLimiter("non-api");
        }

        var apiKey = httpContext.Request.Headers[ApiSecurityOptions.ApiKeyHeaderName].ToString();
        var partitionKey = string.IsNullOrWhiteSpace(apiKey) ? "anonymous" : apiKey;

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = apiSecurityOptions.PermitLimit,
                Window = TimeSpan.FromSeconds(apiSecurityOptions.WindowSeconds),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
});

builder.Services.AddHttpClient<
    IPartnerVerificationService,
    PartnerVerificationService>
(
    client =>
    {
        client.BaseAddress =
            new Uri(partnerVerificationBaseUrl);
        client.DefaultRequestHeaders.Add(
            ApiSecurityOptions.ApiKeyHeaderName,
            apiSecurityOptions.ApiKey);
    })
    .AddResilienceHandler(
        "partner-retry",
        builder =>
        {
            builder.AddRetry(
                new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    DelayGenerator = _ => new ValueTask<TimeSpan?>(
                        TimeSpan.FromSeconds(2) +
                        TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000)))
                });
        });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key needed",
        In = ParameterLocation.Header,
        Name = "X-Api-Key",
        Type = SecuritySchemeType.ApiKey
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

GlobalExceptionHandler.ConfigureExceptionHandler(app);

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<ApiKeyMiddleware>();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
