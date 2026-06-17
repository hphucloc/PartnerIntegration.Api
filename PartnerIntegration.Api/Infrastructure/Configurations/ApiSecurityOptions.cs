namespace PartnerIntegration.Api.Infrastructure.Configurations
{
    public class ApiSecurityOptions
    {
        public const string SectionName = "ApiSecurity";
        public const string ApiKeyHeaderName = "X-Api-Key";

        public string ApiKey { get; set; } = string.Empty;
        public int PermitLimit { get; set; } = 10;
        public int WindowSeconds { get; set; } = 60;
    }
}
