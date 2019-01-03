using System.Net;
using System.IO;
using PackageManagerNS;
using Constants;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NetworkUtilsNS {
    public static class NetworkUtils {
        public class NetworkUtilsException : System.Exception {
            public NetworkUtilsException(string mssg) : base(mssg) {}
        }
        private static readonly string resourcesUrl = "http://localhost:5000/api/";

        public static Stream getResource(ResourceType resourceType, string resourceName) {
            string url = $"{resourcesUrl}resources/{resourceType.ToString()}/{resourceName}";
            
            using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient()) {
                try {
                    Stream stream = client.GetStreamAsync(url).Result;
                    return stream;
                } catch (System.Exception) {
                    throw new NetworkUtilsException("Unable to get requested resource from server");
                }
            }
        }

        public static List<string> getAvailableResources(ResourceType resourceType) {
            string url = $"{resourcesUrl}resources/{resourceType.ToString()}";

            using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient()) {
                try {
                    string jsonArr = client.GetStringAsync(url).Result;
                    List<string> resources = JsonConvert.DeserializeObject<List<string>>(jsonArr);
                    return resources;
                } catch (JsonReaderException) {
                    throw new NetworkUtilsException("Unable to process server response");
                }catch (System.Exception) {
                    throw new NetworkUtilsException("Unable to get requested information from server");
                } 
            }
        }
    }
}