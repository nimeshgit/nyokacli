using Constants;
using System.Collections.Generic;
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
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex);
                throw new NetworkUtilsException(
                    $"Unable to get file for {resourceType} resource {resourceName}"
                );
            }
        }

        public static ResourceVersionsInfoContainer getResourceVersions(ResourceType resourceType, string resourceName)
        {
            string url = resourceVersionsUrl(resourceType, resourceName);

            string serializedInfo;
            try
            {
                serializedInfo = client.GetStringAsync(url).Result;
                System.Console.WriteLine(serializedInfo);
            }

            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex);
                throw new NetworkUtilsException(
                    $"Unable to get list of versions of {resourceType} resource {resourceName} from server"
                );
            }
            try
            {
                ResourceVersionsInfoContainer versionsInfo = ResourceVersionsInfoContainer.deserialize(serializedInfo);

                return versionsInfo;
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex);
                throw new NetworkUtilsException(
                    $"Unable to process server response to request for " +
                    $"list of versions of {resourceType} resource {resourceName}"
                );
            }
        }

        public static ResourceDependencyInfoContainer getResourceDependencies(ResourceType resourceType, string resourceName, string version)
        {
            string url = resourceDependenciesUrl(resourceType, resourceName, version);

            string serializedInfo;

            try
            {
                serializedInfo = client.GetStringAsync(url).Result;
                System.Console.WriteLine(serializedInfo);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex);
                throw new NetworkUtilsException("Unable to get list of package dependencies from server");
            }
            
            try
            {
                ResourceDependencyInfoContainer dependencies = ResourceDependencyInfoContainer.deserialize(serializedInfo);
                return dependencies;
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex);
                throw new NetworkUtilsException("Unable to process server response to request for list of dependencies");
            }
        }

        public static AvailableResourcesInfoContainer getAvailableResources(ResourceType resourceType)
        {
            string url = availableResourcesUrl(resourceType);

            string serialized;
            try
            {
                serialized = client.GetStringAsync(url).Result;
                System.Console.WriteLine(serialized);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex);
                throw new NetworkUtilsException("Unable to get list of available resources from server");
            }
            try
            {
                AvailableResourcesInfoContainer resources = AvailableResourcesInfoContainer.deserialize(serialized);
                return resources;
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex);
                throw new NetworkUtilsException("Unable to process server response to available resources request");
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

            queryString["deps"] = publishDepsInfo.serialize();

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
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex);
                throw new NetworkUtilsException("Unable to publish {resourceType} resource {resourceName} to server");
            }
        }
    }
}
