# PartnerIntegration.Api

## Architecture

This project is a small ASP.NET Core Web API for partner transaction processing.

Main design choices:

- **Thin controller**: `PartnerTransactionsController` only handles HTTP input/output.
- **Service layer**: `TransactionService` contains validation, partner verification, and message publishing flow.
- **Validation layer with FluentValidation**: `PartnerTransactionValidator` centralizes request validation rules.
- **Typed `HttpClient` with Polly-based resilience**: `PartnerVerificationService` calls the internal verification endpoint with retry support for transient failures.
- **Messaging abstraction**: accepted transactions are published to RabbitMQ through `IMessagePublisher` and `IRabbitMqClient`.
- **Global exception handling**: unhandled exceptions are returned as `ProblemDetails`.
- **Basic protection**: the API uses `X-Api-Key` and rate limiting.

## Prerequisites
- **.NET8**
- **Docker Desktop**

Start RabbitMQ first.

From the repository root: PartnerIntegration.Api

```powershell
docker compose up -d
```

RabbitMQ Management UI: wait a minute to browse

- `http://localhost:15672`

Default credentials:

- username: `guest`
- password: `guest`

Swagger will be available at:

- `http://localhost:8080/swagger/index.html`

## Partner Verification Retry

The partner verification call uses a Polly-based retry strategy through the configured HTTP resilience handler.

The mock verification endpoint is intentionally unstable for testing. 
In the current implementation, it has a 30% chance to throw a timeout-related failure and a 70% chance to return a normal response. 
The retry mechanism helps the API recover from these transient failures instead of failing immediately. 
(logic in PartnerVerificationController.cs)

## Test Project

The solution includes a separate test project: `PartnerIntegration.Tests`.

It covers unit test scenarios for:

- validation logic
- Polly retry behavior in partner verification
- transaction service business flow
- RabbitMQ publisher behavior

## Test with Swagger

Use the `POST /api/PartnerTransactions` endpoint.

Required header X-Api-Key:

- `x-api-key`

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
