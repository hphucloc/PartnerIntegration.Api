namespace PartnerIntegration.Api.Models.Events
{
    public record PartnerTransactionAcceptedEvent(
        string PartnerId,
        string TransactionReference,
        decimal Amount,
        string Currency,
        DateTime Timestamp,
        DateTime ProcessedAtUtc);
}
