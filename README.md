# PartnerIntegration.Api

## Architecture

This project is a small ASP.NET Core Web API for partner transaction processing.

Main design choices:

- **Thin controller**: `PartnerTransactionsController` only handles HTTP input/output.
- **Service layer**: `TransactionService` contains validation, partner verification, and message publishing flow.
- **Validation layer**: `PartnerTransactionValidator` centralizes request validation rules.
- **Typed `HttpClient` with Polly-based resilience**: `PartnerVerificationService` calls the internal verification endpoint with retry support for transient failures.
- **Messaging abstraction**: accepted transactions are published to RabbitMQ through `IMessagePublisher` and `IRabbitMqClient`.
- **Global exception handling**: unhandled exceptions are returned as `ProblemDetails`.
- **Basic protection**: the API uses `X-Api-Key` and rate limiting.

## Prerequisites

- **Docker Desktop**
- **.NET 8 SDK**
- **Visual Studio**

## Run the Project in Visual Studio

1. Open the solution in **Visual Studio**.

2. Start RabbitMQ first.

From the repository root:

```powershell
docker compose up -d rabbitmq
```

RabbitMQ Management UI: wait a minute to browse

- `http://localhost:15672`

Default credentials:

- username: `guest`
- password: `guest`

3. In Visual Studio, set `PartnerIntegration.Api` as the startup project.

4. Select the **Docker** launch profile.

5. Run the project.

Swagger will be available at:

- `https://localhost:32775/swagger`

## Partner Verification Retry

The partner verification call uses a Polly-based retry strategy through the configured HTTP resilience handler.

The mock verification endpoint is intentionally unstable for testing. In the current implementation, it has a 30% chance to throw a timeout-related failure and a 70% chance to return a normal response. The retry mechanism helps the API recover from these transient failures instead of failing immediately. (in PartnerVerificationController.cs)

## Test Project

The solution includes a separate test project: `PartnerIntegration.Tests`.

It covers unit test scenarios for:

- validation logic
- Polly retry behavior in partner verification
- transaction service business flow
- RabbitMQ publisher behavior

It also includes integration tests for the transaction endpoint.

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
