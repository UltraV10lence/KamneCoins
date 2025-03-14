using KamneCoins.Http;
using Newtonsoft.Json.Linq;

namespace KamneCoins.Api;

public class Register : ApiEndpoint {
    public static void Execute(HttpRequest request, HttpResponse response) {
        var postRequest = request.JsonPostData;
        var postResponse = new JObject();
        response.DataType.SetJson();
        
        if (postRequest == null) {
            postResponse["code"] = new JValue(-1);
            postResponse["message"] = JValue.CreateString("Cannot parse post request: Invalid json data");
            
            response.Status.SetBadRequest();
            response.WriteJson(postResponse);
            return;
        }

        var username = postRequest["username"];
        var password = postRequest["password"];
        if (username == null) {
            postResponse["code"] = new JValue(-2);
            postResponse["message"] = JValue.CreateString("Username is not provided");
            
            response.Status.SetBadRequest();
            response.WriteJson(postResponse);
            return;
        }
        
        if (password == null) {
            postResponse["code"] = new JValue(-2);
            postResponse["message"] = JValue.CreateString("Password is not provided");
            
            response.Status.SetBadRequest();
            response.WriteJson(postResponse);
            return;
        }

        postResponse["code"] = new JValue(0);
        postResponse["message"] = JValue.CreateString("Account created successfully!");
        
        response.Status.SetOk();
        response.WriteJson(postResponse);
    }
}