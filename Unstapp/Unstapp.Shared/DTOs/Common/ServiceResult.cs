namespace Unstapp.Shared.DTOs.Common
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public ApiErrorResponse? Error { get; set; }
        public static ServiceResult<T> Ok(T data)
        {
            return new ServiceResult<T>
            {
                Success = true,
                Data = data
            };
        }

        public static ServiceResult<T> Fail(int statusCode, string code, string message)
        {
            return new ServiceResult<T>
            {
                Success = false,
                Error = new ApiErrorResponse
                {
                    StatusCode = statusCode,
                    Code = code,
                    Message = message
                }
            };
        }
    }
}
