using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KamneCoins.Http;

public class HttpResponse {
    public Stream RawStream { get; }
    public StreamWriter Writer { get; }
    public StatusCode Status { get; } = new();
    public HttpDataType DataType { get; } = new();
    public Dictionary<string, string> Headers { get; } = [];
    
    public HttpResponse(Stream rawStream) {
        RawStream = rawStream;
        Writer = new StreamWriter(RawStream, System.Text.Encoding.UTF8, leaveOpen: true);
        DataType.SetText();
    }

    public void Redirect(string newUri) {
        Status.SetRedirect();
        Headers.Add("Location", newUri);
    }

    public void WriteJson(JToken json, bool indentation = false) {
        Writer.Write(json.ToString(indentation ? Formatting.Indented : Formatting.None));
    }
    
    public class HttpDataType {
        public string Type { get; private set; } = "*/*";

        public void SetDataType(string rawDataType) {
            Type = rawDataType;
        }

        public void SetText() => SetDataType("text/plain");
        public void SetHtml() => SetDataType("text/html");
        public void SetCss() => SetDataType("text/css");
        public void SetJavaScript() => SetDataType("application/javascript");
        public void SetIcon() => SetDataType("image/x-icon");
        public void SetJpeg() => SetDataType("image/jpeg");
        public void SetPng() => SetDataType("image/png");
        public void SetJson() => SetDataType("application/json");
        public void SetGif() => SetDataType("image/gif");
        public void SetXml() => SetDataType("text/xml");

        public void SetByExtension(string extension) {
            switch (extension) {
                case "html":
                    SetHtml();
                    break;
                case "css":
                    SetCss();
                    break;
                case "js":
                    SetJavaScript();
                    break;
                case "ico":
                    SetIcon();
                    break;
                case "jpeg":
                    SetJpeg();
                    break;
                case "png":
                    SetPng();
                    break;
                case "json":
                    SetJson();
                    break;
                case "gif":
                    SetGif();
                    break;
                case "xml":
                    SetXml();
                    break;
                case "txt":
                    SetText();
                    break;
                default:
                    SetDataType($"text/{extension}");
                    break;
            }
        }
    }
    
    public class StatusCode {
        public int Code { get; private set; }
        public string Message { get; private set; } = string.Empty;

        public void SetOk() => SetParameters(200, "OK");
        public void SetRedirect() => SetParameters(307, "Temporary Redirect");
        public void SetServerError() => SetParameters(500, "Internal Server Error");
        public void SetAccessDenied() => SetParameters(403, "Forbidden");
        public void SetNotFound() => SetParameters(404, "Not Found");
        public void SetBadRequest() => SetParameters(400, "Bad Request");
        public void SetUnauthorized() => SetParameters(401, "Unauthorized");

        public void SetParameters(int code, string message) {
            Code = code;
            Message = message;
        }
    }
}