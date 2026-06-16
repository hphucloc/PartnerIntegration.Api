using PartnerIntegration.Api.Models.Requests;
using PartnerIntegration.Api.Validators;

namespace PartnerIntegration.Tests.Validators;

public class PartnerTransactionValidatorTests
{
    private readonly PartnerTransactionValidator _validator = new();

    [Fact]
    public void Validate_WithValidRequest_ReturnsTrueAndNoErrors()
    {
        var request = new PartnerTransactionRequest
        {
            PartnerId = "PARTNER_001",
            TransactionId = "TX-123",
            Amount = 100,
            Currency = "USD"
        };

        var isValid = _validator.Validate(request, out var errors);

        Assert.True(isValid);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_WithInvalidRequest_ReturnsFalseAndValidationErrors()
    {
        var request = new PartnerTransactionRequest
        {
            PartnerId = string.Empty,
            TransactionId = string.Empty,
            Amount = 0,
            Currency = string.Empty
        };

        var isValid = _validator.Validate(request, out var errors);

        Assert.False(isValid);
        Assert.Contains("PartnerId is required.", errors);
        Assert.Contains("TransactionId is required.", errors);
        Assert.Contains("Amount must be greater than zero.", errors);
        Assert.Contains("Currency is required.", errors);
    }
}
