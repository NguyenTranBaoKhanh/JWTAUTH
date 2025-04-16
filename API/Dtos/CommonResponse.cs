namespace API.Dtos
{
    public class CommonResponse<T>
    {
        public bool Success { get; set; }
        public T Payload { get; set; }
        public ResponseStatus Status { get; set; }
        public ErrorItem Error { get; set; }

        public CommonResponse()
        {
            Success = true;
        }
    }

    public class ResponseStatus
    {
        public ResponseStatus(ResponseCode responseCode)
        {
            ResponseCode = (int)responseCode;
            ResponseCodeDesc = responseCode.ToString();
        }

        public int ResponseCode { get; set; }
        public string ResponseCodeDesc { get; set; }
    }

    public class ErrorItem
    {
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public string RedirectUrl { get; set; }
        public Exception Exception { get; set; }

        public ErrorItem() : this(string.Empty, null, null, null)
        {
        }
        public ErrorItem(string errorMessage, string errorCode) : this(errorMessage, errorCode, null, null)
        {
        }


        public ErrorItem(string message) : this(message, null, null, null)
        {
        }

        public ErrorItem(string message, Exception exception) : this(message, null, null, exception)
        {
        }

        public ErrorItem(string message, string errorCode, string redirectUrl, Exception exception)
        {
            Message = message;
            Exception = exception;
            ErrorCode = errorCode;
            RedirectUrl = redirectUrl;
        }
        
    }
    public enum ResponseCode
        {
            /// <summary>
            /// Success
            /// </summary>
            Success = 1,
            /// <summary>
            /// No authenticate
            /// </summary>
            NoAuthentication = 0,
            /// <summary>
            /// Generic Error
            /// </summary>
            Error = -1
        }
}