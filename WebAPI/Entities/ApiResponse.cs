namespace WebAPI.Entities
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public int StatusCode { get; set; } // Campo para mapear códigos HTTP
        public T? Data { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string message = "Operação realizada com sucesso.", int statusCode = 200) =>
            new() { Success = true, Message = message, Data = data, StatusCode = statusCode };

        public static ApiResponse<T> ErrorResponse(string message, string? errorCode = null, int statusCode = 400) =>
            new() { Success = false, Message = message, ErrorCode = errorCode, StatusCode = statusCode };
    }
}
