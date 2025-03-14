using System.Reflection;
using KamneCoins.Http;

namespace KamneCoins;

internal static class Program {
    public const string WorkingDirectory = "D:/Programming/C#/Projects/KamneCoins/KamneCoins/bin/Debug/net9.0/DebugServer";
    
    public static async Task Main() {
        await HttpServer.StartServer(RequestConsumer, false);
    }

    public static void RequestConsumer(HttpRequest request, HttpResponse response) {
        if (request.RawRequestPath != request.RequestPath) {
            response.Redirect(request.RequestPath);
            return;
        }
        
        var fileToOpen = request.RequestAbsolutePath;
        if (request.IsDirectory) {
            fileToOpen += "index.html";
        }

        if (!AccessControl.HasAccess(request)) {
            response.Status.SetAccessDenied();
            response.Writer.WriteLine("Sorry, but you do not have access to this page.");
            return;
        }

        var file = new FileInfo(fileToOpen);
        if (!file.Exists) {
            response.Status.SetNotFound();
            response.Writer.WriteLine("Cannot find requesting page.");
            return;
        }

        var extension = Path.GetExtension(fileToOpen)[1..].ToLowerInvariant();
        if (extension == "csref") {
            string refTo;
            using (var reader = new StreamReader(file.OpenRead())) {
                refTo = reader.ReadToEnd();
            }

            var type = Assembly.GetExecutingAssembly().GetType(refTo);
            if (Equals(type, null) || !typeof(ApiEndpoint).IsAssignableFrom(type)) {
                response.Status.SetServerError();
                response.Writer.WriteLine("Sorry, but this reference is invalid");
                return;
            }
            
            var method = type.GetMethod("Execute", BindingFlags.Static | BindingFlags.Public, [typeof(HttpRequest), typeof(HttpResponse)]);
            if (Equals(method, null)) {
                response.Status.SetServerError();
                response.Writer.WriteLine("Type was found, but execution entry point is missing");
                return;
            }
            
            method.Invoke(null, [request, response]);
            return;
        }
        
        response.Status.SetOk();
        response.DataType.SetByExtension(extension);
        using (var fileStream = file.OpenRead()) {
            fileStream.CopyTo(response.RawStream);
        }
    }
}