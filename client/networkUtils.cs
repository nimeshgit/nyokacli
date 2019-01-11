using Constants;
using System.Collections.Generic;
using Newtonsoft.Json;
using InfoTransferContainers;
using System.Linq;

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

        private static readonly string apiUrl = "http://localhost:5000/api";
        
        private static string getApiUrl => $"{apiUrl}/getresources";
        private static string postApiUrl => $"{apiUrl}/postresources";

        private static readonly System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

        public static System.IO.Stream getResource(ResourceType resourceType, string resourceName, string version)
        {
            string url = $"{getApiUrl}/{resourceType.ToString().ToLower()}/{resourceName}/versions/{version}/file";
            
            try
            {
                System.IO.Stream stream = client.GetStreamAsync(url).Result;
                return stream;
            }
            catch (System.Exception)
            {
                throw new NetworkUtilsException("Unable to get requested resource from server");
            }
        }

        public static ResourceVersionsInfoContainer getResourceVersions(ResourceType resourceType, string resourceName)
        {
            string url = $"{getApiUrl}/{resourceType.ToString().ToLower()}/{resourceName}/versions";

            try
            {
                string resourceJson = client.GetStringAsync(url).Result;
                ResourceVersionsInfoContainer versionsInfo = JsonConvert.DeserializeObject<ResourceVersionsInfoContainer>(resourceJson);
                
                return versionsInfo;
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

        public static ResourceDependencyInfoContainer getResourceDependencies(ResourceType resourceType, string resourceName, string version)
        {
            string url = $"{getApiUrl}/{resourceType.ToString().ToLower()}/{resourceName}/versions/{version}/dependencies";

            try
            {
                string resourceJson = client.GetStringAsync(url).Result;
                ResourceDependencyInfoContainer dependencies = JsonConvert.DeserializeObject<ResourceDependencyInfoContainer>(resourceJson);
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
            string url = $"{getApiUrl}/{resourceType.ToString().ToLower()}";

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

        public static void publishResource(
            System.IO.FileStream fileStream,
            ResourceType resourceType,
            string resourceName,
            string version,
            PublishDepsInfoContainer publishDepsInfo)
        {
            System.Collections.Specialized.NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

            queryString["deps"] = JsonConvert.SerializeObject(publishDepsInfo);

            string url = $"{postApiUrl}/{resourceType.ToString().ToLower()}/{resourceName}/versions/{version}/post?{queryString.ToString()}";

            try
            {
                using (var fileContent = new System.Net.Http.StreamContent(fileStream))
                {
                    // var fileNameOnly = Path.GetFileName(fileName);
                    // var fileContent = new System.Net.Http.StreamContent(File.OpenRead(fileName));
                    fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                    {
                        Name = $"files[{resourceName}]",
                        FileName = resourceName
                    };
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                    // content.Add(new System.Net.Http.StringContent("gettext"), "type");


                    System.Net.Http.HttpResponseMessage statusResult = client.PostAsync(url, fileContent).Result;
                    
                    if (!statusResult.IsSuccessStatusCode)
                    {
                        System.Console.WriteLine("not success status code");
                        throw new NetworkUtilsException("Unsuccessful");
                    }
                }
                // client.PostAsync(url, )
                // System.Net.Webc
                // client.PostAsync(url, new HttpContent)
            }
            catch (System.Exception)
            {
                throw new NetworkUtilsException("Unable to post resource to server");
            }
            // postresources/{resourceType}/{resourceName}/versions/{version}/post
        }
    }
}