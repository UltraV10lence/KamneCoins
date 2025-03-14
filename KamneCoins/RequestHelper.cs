using KamneCoins.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KamneCoins;

public static class RequestHelper {
    public static JToken? ReadFromStream(Stream stream) {
        try {
            using (var reader = new StreamReader(stream))
            using (var jReader = new JsonTextReader(reader)) {
                return JToken.Load(jReader);
            }
        } catch { }
        
        return null;
    }
/*
    public static bool IsPresent(JToken? json, HttpResponse response, string message) {
        if (json != null) return true;
        
        response.Status.SetBadRequest();
        response.DataType.SetJson();
        
        return false;
    }*/
}