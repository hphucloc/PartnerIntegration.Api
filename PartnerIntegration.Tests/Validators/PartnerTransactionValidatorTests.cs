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
            PartnerId = "P-1001",
            TransactionReference = "TX-123",
            Amount = 100,
            Currency = "USD"
        };

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithInvalidRequest_ReturnsFalseAndValidationErrors()
    {
        var request = new PartnerTransactionRequest
        {
            PartnerId = string.Empty,
            TransactionReference = string.Empty,
            Amount = 0,
            Currency = string.Empty
        };

        var result = _validator.Validate(request);
        var errors = result.Errors.Select(error => error.ErrorMessage).ToList();

        Assert.False(result.IsValid);
        Assert.Contains("PartnerId is required.", errors);
        Assert.Contains("TransactionReference is required.", errors);
        Assert.Contains("Amount must be greater than zero.", errors);
        Assert.Contains("Currency is required.", errors);
    }

    [Fact]
    public void Validate_WithInvalidCurrency_ReturnsCurrencyValidationError()
    {
        var request = new PartnerTransactionRequest
        {
            PartnerId = "P-1001",
            TransactionReference = "TX-123",
            Amount = 100,
            Currency = "ZZZ"
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage == "Currency is invalid.");
    }

    [Fact]
    public void Validate_WithLowerCaseValidCurrency_ReturnsTrue()
    {
        var request = new PartnerTransactionRequest
        {
            PartnerId = "P-1001",
            TransactionReference = "TX-123",
            Amount = 100,
            Currency = "usd"
        };

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }
}
