namespace PlugIn.Contracts.Responses
{
	public class BaseResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }
    }
}
