using System.Net;
using System.Net.Sockets;
using System.Web;

namespace KamneCoins.Http;

public static class HttpServer {
    public delegate void RequestConsumer(HttpRequest request, HttpResponse response);
    
    public static async Task StartServer(RequestConsumer consumer) {
        var listener = new TcpListener(IPAddress.Any, 80);
        listener.Start();

        while (true) {
            var client = await listener.AcceptTcpClientAsync();

            _ = Task.Factory.StartNew(() => {
                try {
                    ConsumeConnection(client, consumer);
                    client.Close();
                } catch { }
            });
        }
    }

    private static void ConsumeConnection(TcpClient client, RequestConsumer consumer) {
        var shouldCloseConnection = false;
        var clientIp = ((IPEndPoint) client.Client.RemoteEndPoint!).Address.ToString();
        
        using (var stream = client.GetStream())
        using (var reader = new Utf8StreamReader(stream))
        using (var writer = new StreamWriter(stream)) {
            writer.AutoFlush = true;
            
            while (!shouldCloseConnection) {
                var requestLine = reader.ReadLine();
                if (string.IsNullOrEmpty(requestLine))
                    continue;

                var tokens = requestLine.Split(' ');
                if (tokens.Length < 3) {
                    writer.WriteLine("HTTP/1.1 400 Bad request");
                    writer.WriteLine("Connection: close");
                    shouldCloseConnection = true;
                    continue;
                }

                var requestMethod = tokens[0];
                (var path, var parameters) = ExtractPath(tokens[1]);

                var headers = new Dictionary<string, string>();
                string header;
                while (!string.IsNullOrEmpty(header = reader.ReadLine()!)) {
                    AddHeaderToDictionary(headers, header);
                }
                
                shouldCloseConnection = headers.TryGetValue("Connection", out var close) && close == "close";
                using (var requestContents = new MemoryStream()) {
                    if (headers.TryGetValue("Content-Length", out var requestContentLength))
                        ReadRequestContents(requestContents, stream, requestContentLength);
                    
                    requestContents.Seek(0, SeekOrigin.Begin);
                    var request = new HttpRequest(clientIp, requestMethod, path, headers, parameters, requestContents);
                    SendResponse(consumer, request, writer, stream, shouldCloseConnection);
                }
            }
        }
    }

    private static void ReadRequestContents(MemoryStream requestContents, Stream stream, string requestContentLength) {
        var length = int.Parse(requestContentLength);

        var buf = new byte[length];
        stream.ReadExactly(buf);
        requestContents.Write(buf);
    }

    public static (string path, Dictionary<string, string> parameters) ExtractPath(string fullPath) {
        var paramsSplitter = fullPath.IndexOf('?');
        if (paramsSplitter < 0) return (fullPath, []);

        var path = HttpUtility.HtmlDecode(fullPath[..paramsSplitter]);
        
        var keyValues = fullPath[(paramsSplitter + 1)..].Split('&').Select(kv => {
            var splitter = kv.IndexOf('=');
            if (splitter < 0) return [];
            
            return (string[]) [kv[..splitter], kv[(splitter + 1)..]];
        }).ToList();
        
        return keyValues.Any(kv => kv.Length < 2) ? (path, []) :
                   (path, keyValues.ToDictionary(kv => kv[0], kv => kv[1]));

    }

    public static void SendResponse(RequestConsumer consumer, HttpRequest request, StreamWriter writer, Stream stream, bool shouldCloseConnection) {
        using (var buffer = new MemoryStream(1024)) {
            var response = new HttpResponse(buffer);
            response.SetOk();

            try {
                consumer.Invoke(request, response);
                response.Writer.Flush();
                response.Writer.Dispose();
            } catch (Exception e) {
                Console.WriteLine($"Error while executing consumer: {e}");
                response.SetServerError();
            }

            response.Headers.TryAdd("Content-Type", response.DataType switch {
                HttpDataType.Html => "text/html",
                HttpDataType.Text => "text/plain",
                HttpDataType.Json => "application/json",
                HttpDataType.Jpeg => "image/jpeg",
                HttpDataType.Png => "image/png",
                _ => "text/plain"
            });

            response.Headers.TryAdd("Content-Length", buffer.Length.ToString());
            response.Headers.TryAdd("Connection", shouldCloseConnection ? "close" : "keep-alive");
                    
            writer.WriteLine($"HTTP/1.1 {response.StatusCode} {response.StatusCodeMessage}");
            foreach (var responseHeader in response.Headers)
                writer.WriteLine(responseHeader.Key + ": " + responseHeader.Value);
            writer.WriteLine();

            buffer.Seek(0, SeekOrigin.Begin);
            buffer.CopyTo(stream);
            stream.Flush();
        }
    }

    private static void AddHeaderToDictionary(Dictionary<string, string> headers, string rawHeader) {
        var separator = rawHeader.IndexOf(':');
        if (separator < 0) return;

        var header = rawHeader[..separator];
        var value = rawHeader[(separator + 1)..];
        value = value.TrimStart();
        headers.Add(header, value);
    }
}