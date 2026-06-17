using System.Globalization;
using FluentValidation;
using PartnerIntegration.Api.Models.Requests;

namespace PartnerIntegration.Api.Validators
{
    public class PartnerTransactionValidator : AbstractValidator<PartnerTransactionRequest>
    {
        private static readonly HashSet<string> ValidCurrencies = CultureInfo
            .GetCultures(CultureTypes.SpecificCultures)
            .Select(culture => new RegionInfo(culture.Name).ISOCurrencySymbol)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        public PartnerTransactionValidator()
        {
            RuleFor(x => x.PartnerId)
                .NotEmpty()
                .WithMessage("PartnerId is required.");

            RuleFor(x => x.TransactionReference)
                .NotEmpty()
                .WithMessage("TransactionReference is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than zero.");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("Currency is required.")
                .Must(BeValidCurrency)
                .WithMessage("Currency is invalid.");
        }

        private static bool BeValidCurrency(string? currency)
        {
            return !string.IsNullOrWhiteSpace(currency) && ValidCurrencies.Contains(currency);
        }
    }
}
