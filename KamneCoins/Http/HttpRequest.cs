namespace KamneCoins.Http;

public class HttpRequest {
    public string Ip { get; }
    public string RequestMethod { get; }
    public string RequestPath { get; }
    public Dictionary<string, string> Headers { get; }
    public Dictionary<string, string> RequestParams { get; }
    public Stream RequestContents { get; }
    public (string absolutePath, bool isDirectory) RequestAbsolutePath { get; }

    public HttpRequest(string ip, string requestMethod, string requestPath, Dictionary<string, string> headers, Dictionary<string, string> requestParams, Stream requestContents) {
        Ip = ip;
        RequestMethod = requestMethod;
        RequestPath = InitRequestPath(requestPath);
        Headers = headers;
        RequestParams = requestParams;
        RequestContents = requestContents;
        RequestAbsolutePath = InitAbsolutePath();
    }

    private string InitRequestPath(string requestPath) {
        requestPath = Uri.UnescapeDataString(requestPath);
        requestPath = requestPath.Replace('\\', '/');
        return requestPath;
    }

    private (string absolutePath, bool isDirectory) InitAbsolutePath() {
        var path = RequestPath;

        var isDirectory = true;
        var canBeExtension = true;
        path = new string(path.ToCharArray().Reverse().Where(c => {
            switch (c) {
                case '.' when canBeExtension && isDirectory:
                    isDirectory = false;
                    return true;
                case '/':
                    canBeExtension = false;
                    return true;
                default:
                    return char.IsAsciiLetterOrDigit(c);
            }
        }).Reverse().ToArray());

        return (Program.WorkingDirectory + path + (isDirectory && path.Last() != '/' ? '/' : ""), isDirectory);
    }
}