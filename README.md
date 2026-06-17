# PartnerIntegration.Api

## Architecture

This project is a small ASP.NET Core Web API for partner transaction processing.

Main design choices:

- **controller**: `PartnerTransactionsController` only handles HTTP input/output.
- **Service layer**: `TransactionService` contains the transaction flow.
- **Validation layer**: `PartnerTransactionValidator` centralizes request validation rules.
- **Typed `HttpClient`**: `PartnerVerificationService` calls the internal partner verification endpoint with retry/resilience support.
- **Messaging abstraction**: accepted transactions are published to RabbitMQ through `IMessagePublisher` and `IRabbitMqClient`.
- **Global exception handling**: unhandled exceptions are returned as `ProblemDetails`.
- **Basic protection**: the API uses `X-Api-Key` and rate limiting.

## Run the Project

1. Start RabbitMQ:

```powershell
docker compose up -d
```

2. Run the API:

```powershell
dotnet run --project .\PartnerIntegration.Api\PartnerIntegration.Api.csproj --launch-profile https
```

3. Open Swagger:

- `https://localhost:7079/swagger`

## Test with Swagger

Use the `POST /api/PartnerTransactions` endpoint in Swagger.

Required header:

- `X-Api-Key: x-api-key`

Example request body:

```json
{
  "partnerId": "P-1001",
  "transactionReference": "TXN-99823",
  "amount": 250.00,
  "currency": "USD",
  "timestamp": "2024-05-10T14:30:00Z"
}
```

Expected behavior:

- valid request -> `200 OK`
- invalid API key -> `401 Unauthorized`
- too many requests -> `429 Too Many Requests`
- invalid partner -> `400 Bad Request`
