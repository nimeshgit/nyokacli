using System.Net;
using System.IO;
using PackageManagerNS;
using Constants;

namespace NetworkUtilsNS {
    public static class NetworkUtils {
        public class NetworkUtilsException : System.Exception {
            public NetworkUtilsException(string mssg) : base(mssg) {}
        }
        private static readonly string resourcesUrl = "http://localhost:5001/resources";

        public static Stream getResource(ResourceType resourceType, string resourceName) {
            string url = $"{resourcesUrl}/resources/{resourceType.ToString()}/{resourceName}";
            System.Console.WriteLine(url);
            
            using (System.Net.WebClient client = new System.Net.WebClient()) {
                try {
                    Stream stream = client.OpenRead(url);
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