using PartnerIntegration.Api.Models.Requests;

namespace PartnerIntegration.Api.Validators
{
    public class PartnerTransactionValidator
    {
        public bool Validate(PartnerTransactionRequest request, out IEnumerable<string> errors)
        {
            var validationErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.PartnerId))
            {
                validationErrors.Add("PartnerId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.TransactionId))
            {
                validationErrors.Add("TransactionId is required.");
            }

            if (request.Amount <= 0)
            {
                validationErrors.Add("Amount must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(request.Currency))
            {
                validationErrors.Add("Currency is required.");
            }

            errors = validationErrors;
            return !validationErrors.Any();
        }
    }
}
