namespace WebAPI.Entities
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public T? Data { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string message = "Operação realizada com sucesso.") =>
            new() { Success = true, Message = message, Data = data };

        public static ApiResponse<T> ErrorResponse(string message, string? errorCode = null) =>
            new() { Success = false, Message = message, ErrorCode = errorCode };
    }
}
