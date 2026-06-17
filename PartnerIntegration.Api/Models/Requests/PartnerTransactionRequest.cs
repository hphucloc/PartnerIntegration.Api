namespace PartnerIntegration.Api.Models.Requests
{
    public class PartnerTransactionRequest
    {
        public string PartnerId { get; set; } = string.Empty;
        public string TransactionReference { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
