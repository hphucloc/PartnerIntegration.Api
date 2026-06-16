using PartnerIntegration.Api.Services.Implementations;

namespace PartnerIntegration.Tests.Services;

public class PartnerVerificationServiceTests
{
    private readonly PartnerVerificationService _service = new();

    [Fact]
    public async Task VerifyPartnerAsync_WithPartnerPrefix_ReturnsTrue()
    {
        var result = await _service.VerifyPartnerAsync("PARTNER_123");

        Assert.True(result);
    }

    [Fact]
    public async Task VerifyPartnerAsync_WithInvalidPartnerId_ReturnsFalse()
    {
        var result = await _service.VerifyPartnerAsync("INVALID_123");

        Assert.False(result);
    }
}
