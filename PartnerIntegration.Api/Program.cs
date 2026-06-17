using Microsoft.Extensions.Http.Resilience;
using PartnerIntegration.Api.Infrastructure.Configurations;
using PartnerIntegration.Api.Infrastructure.Exceptions;
using PartnerIntegration.Api.Messaging;
using PartnerIntegration.Api.Services.Implementations;
using PartnerIntegration.Api.Services.Interfaces;
using Polly;

var builder = WebApplication.CreateBuilder(args);
var partnerVerificationBaseUrl =
    builder.Configuration[
        builder.Environment.IsDevelopment()
            ? "PartnerVerification:DevelopmentBaseUrl"
            : "PartnerVerification:BaseUrl"]
    ?? throw new InvalidOperationException("PartnerVerification:BaseUrl is not configured.");

// Add services to the container.
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RabbitMqOptions>>().Value);
builder.Services.AddSingleton<IRabbitMqClient, RabbitMqClient>();
builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
builder.Services.AddScoped<IPartnerVerificationService, PartnerVerificationService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

builder.Services.AddHttpClient<
    IPartnerVerificationService,
    PartnerVerificationService>
(
    client =>
    {
        client.BaseAddress =
            new Uri(partnerVerificationBaseUrl);
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
builder.Services.AddSwaggerGen();

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

app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
