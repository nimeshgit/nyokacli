using System.Net;
using System.IO;
using PackageManagerNS;
using Constants;
using System.Collections.Generic;
using Newtonsoft.Json;
using InfoTransferContainers;

namespace NetworkUtilsNS
{
    public static class NetworkUtils
    {
        public class NetworkUtilsException : System.Exception
        {
            public NetworkUtilsException(string mssg) 
            : base(mssg)
            {
            }
        }
        private static readonly string resourcesUrl = "http://localhost:5000/api/resources";

        private static readonly System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

        public static Stream getResource(ResourceType resourceType, string resourceName, string version)
        {
            string url = $"{resourcesUrl}/{resourceType.ToString()}/{resourceName}/{version}";
            
            try
            {
                Stream stream = client.GetStreamAsync(url).Result;
                return stream;
            }
            catch (System.Exception)
            {
                throw new NetworkUtilsException("Unable to get requested resource from server");
            }
        }

        public static ResourceInfoContainer getResourceInfo(ResourceType resourceType, string resourceName, string version)
        {
            string url = $"{resourcesUrl}/{resourceType.ToString()}/{resourceName}/{version}/dependencies";

            try
            {
                string resourceJson = client.GetStringAsync(url).Result;
                ResourceInfoContainer dependencies = JsonConvert.DeserializeObject<ResourceInfoContainer>(resourceJson);
                return dependencies;
            }
            catch (JsonReaderException)
            {
                throw new NetworkUtilsException("Unable to process server response");
            }
            catch (System.Exception)
            {
                throw new NetworkUtilsException("Unable to get requested information from server");
            }
        }

        public static Dictionary<string, FileInfoTransferContainer> getAvailableResources(ResourceType resourceType)
        {
            string url = $"{resourcesUrl}/{resourceType.ToString()}";

            try
            {
                string jsonArr = client.GetStringAsync(url).Result;
                Dictionary<string, FileInfoTransferContainer> resources = JsonConvert.DeserializeObject<Dictionary<string, FileInfoTransferContainer>>(jsonArr);
                return resources;
            }
            catch (JsonReaderException)
            {
                throw new NetworkUtilsException("Unable to process server response");
            }
            catch (System.Exception)
            {
                throw new NetworkUtilsException("Unable to get requested information from server");
            }
        }
    }
}