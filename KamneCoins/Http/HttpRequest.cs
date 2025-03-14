using Newtonsoft.Json.Linq;

namespace KamneCoins.Http;

public class HttpRequest {
    public string Ip { get; }
    public string RequestMethod { get; }
    public string RawRequestPath { get; }
    public string RequestPath { get; }
    public Dictionary<string, string> Headers { get; }
    public Dictionary<string, string> RequestParams { get; }
    public Stream RequestContents { get; }
    public JToken? JsonPostData => RequestHelper.ReadFromStream(RequestContents);

    public bool IsDirectory { get; }
    public string RequestAbsolutePath { get; }

    public HttpRequest(string ip, string requestMethod, string requestPath, Dictionary<string, string> headers, Dictionary<string, string> requestParams, Stream requestContents) {
        Ip = ip;
        RequestMethod = requestMethod;
        RawRequestPath = requestPath;
        (RequestPath, IsDirectory) = InitRequestPath(requestPath);
        Headers = headers;
        RequestParams = requestParams;
        RequestContents = requestContents;
        RequestAbsolutePath = InitAbsolutePath(RequestPath);
    }

    private static (string requestPath, bool isDirectory) InitRequestPath(string requestPath) {
        requestPath = requestPath.Replace('\\', '/');

        var isDirectory = true;
        var isFileName = true;
        var canBeExtension = false;
        requestPath = new string(requestPath.ToCharArray().Reverse().Where(c => {
            switch (c) {
                case '.' when isFileName && canBeExtension:
                    isDirectory = false;
                    return true;
                case '/':
                    isFileName = false;
                    return true;
                default:
                    if (!char.IsAsciiLetterOrDigit(c)) return false;
                    canBeExtension = true;
                    return true;
            }
        }).Reverse().ToArray());
        
        
        return (requestPath + (isDirectory && requestPath.Last() != '/' ? '/' : ""), isDirectory);
    }

    private static string InitAbsolutePath(string requestPath) {
        return Program.WorkingDirectory + requestPath;
    }
}