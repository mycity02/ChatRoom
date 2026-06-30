namespace ChatRoom.Server.Dto
{
    /// <summary>
    /// 统一 API 响应格式
    /// </summary>
    public class ApiResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public static ApiResult Ok(string message = "")
        {
            return new ApiResult { Success = true, Message = message };
        }

        public static ApiResult Fail(string message)
        {
            return new ApiResult { Success = false, Message = message };
        }
    }

    /// <summary>
    /// 带数据的 API 响应
    /// </summary>
    public class ApiResult<T> : ApiResult
    {
        public T? Data { get; set; }

        public static ApiResult<T> Ok(T data, string message = "")
        {
            return new ApiResult<T> { Success = true, Message = message, Data = data };
        }

        public new static ApiResult<T> Fail(string message)
        {
            return new ApiResult<T> { Success = false, Message = message };
        }
    }
}
