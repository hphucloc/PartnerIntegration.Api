namespace PartnerIntegration.Api.Models.Responses
{
    public record ApiResponse<T>(bool Success, T? Data, IEnumerable<string>? Errors = null)
    {
        public static ApiResponse<T> SuccessResponse(T data) => new(true, data, Enumerable.Empty<string>());
        public static ApiResponse<T> ErrorResponse(IEnumerable<string> errors) => new(false, default, errors);
    }
}
