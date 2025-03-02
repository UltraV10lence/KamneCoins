namespace KamneCoins.Http;

public class HttpResponse {
    public Stream RawStream { get; }
    public StreamWriter Writer { get; }
    public int StatusCode { get; private set; }
    public string StatusCodeMessage { get; private set; } = string.Empty;
    public Dictionary<string, string> Headers { get; } = [];
    public HttpDataType DataType { get; set; } = HttpDataType.Text;
    
    public HttpResponse(Stream rawStream) {
        RawStream = rawStream;
        Writer = new StreamWriter(RawStream, System.Text.Encoding.UTF8, leaveOpen: true);
    }

    public void SetOk() => SetCodes(200, "OK");
    public void SetAccessDenied() => SetCodes(403, "Forbidden");
    public void SetNotFound() => SetCodes(404, "Not Found");
    public void SetBadRequest() => SetCodes(400, "Bad Request");
    public void SetServerError() => SetCodes(500, "Internal Server Error");

    public void SetCodes(int code, string message) {
        StatusCode = code;
        StatusCodeMessage = message;
    }
}