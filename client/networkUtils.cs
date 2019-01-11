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

        private static string resourceUrlSection(ResourceType resourceType)
        {
            if (resourceType == ResourceType.Code) return "code";
            if (resourceType == ResourceType.Data) return "data";
            if (resourceType == ResourceType.Model) return "model";
            throw new NetworkUtilsException("Could not form request to server");
        }

        private static string resourceFileUrl(ResourceType resourceType, string resourceName, string version)
        {
            return $"{getApiUrl}/{resourceUrlSection(resourceType)}/{resourceName}/versions/{version}/file";
        }

        private static string resourceVersionsUrl(ResourceType resourceType, string resourceName)
        {
            return $"{getApiUrl}/{resourceUrlSection(resourceType)}/{resourceName}/versions";
        }

        private static string resourceDependenciesUrl(ResourceType resourceType, string resourceName, string version)
        {
            return $"{getApiUrl}/{resourceUrlSection(resourceType)}/{resourceName}/versions/{version}/dependencies";
        }

        private static string availableResourcesUrl(ResourceType resourceType)
        {
            return $"{getApiUrl}/{resourceUrlSection(resourceType)}";
        }

        private static string resourcePostUrl(ResourceType resourceType, string resourceName, string version)
        {
            return $"{postApiUrl}/{resourceUrlSection(resourceType)}/{resourceName}/versions/{version}/post";
        }

        private static readonly System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

        public static System.IO.Stream getResource(ResourceType resourceType, string resourceName, string version)
        {
            string url = resourceFileUrl(resourceType, resourceName, version);

            try
            {
                System.IO.Stream stream = client.GetStreamAsync(url).Result;
                return stream;
            }
            catch (System.Exception)
            {
                throw new NetworkUtilsException(
                    $"Unable to get file for {resourceType} resource {resourceName}"
                );
            }
        }

        public static ResourceVersionsInfoContainer getResourceVersions(ResourceType resourceType, string resourceName)
        {
            string url = resourceVersionsUrl(resourceType, resourceName);

            try
            {
                string resourceJson = client.GetStringAsync(url).Result;
                ResourceVersionsInfoContainer versionsInfo = JsonConvert.DeserializeObject<ResourceVersionsInfoContainer>(resourceJson);

                return versionsInfo;
            }
            catch (JsonReaderException)
            {
                throw new NetworkUtilsException(
                    $"Unable to process server response to request for " +
                    $"list of versions of {resourceType} resource {resourceName}"
                );
            }
            catch (System.Exception)
            {
                throw new NetworkUtilsException(
                    $"Unable to get list of versions of {resourceType} resource {resourceName} from server"
                );
            }
        }

        public static ResourceDependencyInfoContainer getResourceDependencies(ResourceType resourceType, string resourceName, string version)
        {
            string url = resourceDependenciesUrl(resourceType, resourceName, version);

            try
            {
                string resourceJson = client.GetStringAsync(url).Result;
                ResourceDependencyInfoContainer dependencies = JsonConvert.DeserializeObject<ResourceDependencyInfoContainer>(resourceJson);
                return dependencies;
            }
            catch (JsonReaderException)
            {
                throw new NetworkUtilsException("Unable to process server response to request for list of dependencies");
            }
            catch (System.Exception)
            {
                throw new NetworkUtilsException("Unable to get list of package dependencies from server");
            }
        }

        public static Dictionary<string, FileInfoTransferContainer> getAvailableResources(ResourceType resourceType)
        {
            string url = availableResourcesUrl(resourceType);

            try
            {
                string jsonArr = client.GetStringAsync(url).Result;
                Dictionary<string, FileInfoTransferContainer> resources = JsonConvert.DeserializeObject<Dictionary<string, FileInfoTransferContainer>>(jsonArr);
                return resources;
            }
            catch (JsonReaderException)
            {
                throw new NetworkUtilsException("Unable to process server response to available resources request");
            }
            catch (System.Exception)
            {
                throw new NetworkUtilsException("Unable to get list of available resources from server");
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

            string url = resourcePostUrl(resourceType, resourceName, version) + "?" + queryString.ToString();

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
            }
            catch (System.Exception)
            {
                throw new NetworkUtilsException("Unable to publish {resourceType} resource {resourceName} to server");
            }
        }
    }
}
