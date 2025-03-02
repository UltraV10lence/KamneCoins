using System.Reflection;
using KamneCoins.Http;

namespace KamneCoins;

internal static class Program {
    public const string WorkingDirectory = "D:/Programming/C#/Projects/KamneCoins/KamneCoins/bin/Debug/net9.0/DebugServer";
    
    public static async Task Main() {
        await HttpServer.StartServer(RequestConsumer);
    }

    public static void RequestConsumer(HttpRequest request, HttpResponse response) {
        (var fileToOpen, var isDirectory) = request.RequestAbsolutePath;
        if (isDirectory) {
            fileToOpen += "index.html";
        }

        if (isDirectory && !request.RequestPath.EndsWith('/')) {
            response.SetCodes(307, "Temporary redirect");
            response.Headers.Add("Location", $"{request.RequestPath}/");
            return;
        }

        if (!AccessControl.HasAccessTo(request)) {
            response.SetAccessDenied();
            response.DataType = HttpDataType.Text;
            response.Writer.WriteLine("Sorry, but you have no access to this page.");
            return;
        }

        var file = new FileInfo(fileToOpen);
        if (!file.Exists) {
            response.SetNotFound();
            response.DataType = HttpDataType.Text;
            response.Writer.WriteLine("Cannot find requesting page.");
            return;
        }

        var extension = Path.GetExtension(fileToOpen);
        if (extension == ".csref") {
            string refTo;
            using (var reader = new StreamReader(file.OpenRead())) {
                refTo = reader.ReadToEnd();
            }

            var type = Assembly.GetExecutingAssembly().GetType(refTo);
            if (Equals(type, null)) {
                response.SetServerError();
                response.DataType = HttpDataType.Text;
                response.Writer.WriteLine("Sorry, but this reference is invalid");
                return;
            }
            
            var method = type.GetMethod("Execute", BindingFlags.Static | BindingFlags.Public, [typeof(HttpRequest), typeof(HttpResponse)]);
            if (Equals(method, null)) {
                response.SetServerError();
                response.DataType = HttpDataType.Text;
                response.Writer.WriteLine("Type was found, but execution entry point is missing");
                return;
            }
            
            method.Invoke(null, [request, response]);
            return;
        }
        
        response.SetOk();
        response.Headers.Add("Content-Type", "text/" + extension[1..].ToLowerInvariant());
        using (var fileStream = file.OpenRead()) {
            fileStream.CopyTo(response.RawStream);
        }
    }
}