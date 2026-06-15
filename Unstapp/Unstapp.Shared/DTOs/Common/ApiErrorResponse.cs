namespace Unstapp.Shared.DTOs.Common
{
    public class ApiErrorResponse
    {
        public int StatusCode { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
