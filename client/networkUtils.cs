using System.Net.Http;

namespace NetworkUtilsNS {
    public static class NetworkUtils {
        private static HttpClient _client = null;
        private static HttpClient client {
            get {
                if (_client == null) return (_client = new HttpClient());
                else return _client;
            }
        }

        private static readonly string baseUrl = "0.0.0.0:5000";
    }
}