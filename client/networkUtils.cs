using System.Net;
using System.IO;
using PackageManagerNS;
using Constants;

namespace NetworkUtilsNS {
    public static class NetworkUtils {
        public class NetworkUtilsException : System.Exception {
            public NetworkUtilsException(string mssg) : base(mssg) {}
        }
        private static readonly string baseUrl = "http://localhost:5050";

        public static Stream getResource(ResourceType resourceType, string resourceName) {
            string url = $"{baseUrl}/getResource/{resourceType}/{resourceName}";
            
            using (System.Net.WebClient client = new System.Net.WebClient()) {
                try {
                    Stream stream = client.OpenRead("http://yoururl/test.txt");
                    return stream;
                } catch (System.Net.WebException) {
                    throw new NetworkUtilsException("Unable to get requested resource from server");
                } catch (System.Exception) {
                    throw new NetworkUtilsException("Unknown error retrieving resource");
                }
            }
        }
    }
}