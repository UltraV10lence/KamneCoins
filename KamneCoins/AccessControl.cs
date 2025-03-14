using KamneCoins.Http;

namespace KamneCoins;

public class AccessControl {
    public static bool HasAccess(HttpRequest request) {
        return request.Ip is "127.0.0.1" or "192.168.0.100" or "5.16.22.100";
    }
}