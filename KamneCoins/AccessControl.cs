using KamneCoins.Http;

namespace KamneCoins;

public class AccessControl {
    public static bool HasAccessTo(HttpRequest request) {
        return request.Ip == "127.0.0.1";
    }
}