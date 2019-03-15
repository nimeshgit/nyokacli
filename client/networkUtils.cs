using Constants;
using System.Collections.Generic;
using InfoTransferContainers;
using System.Linq;
using FSOpsNS;
using CLIInterfaceNS;

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

        private static string baseServerUrl(string prefix)
        {
            if (FSOps.remoteServerConfigFileExists())
            {
                return FSOps.unsafeGetRemoteServerConfigString(prefix);
            }
            else
            {
                return "http://localhost:5000";
            }
        }

        private static string getApiUrl(string prefix) => $"{baseServerUrl(prefix)}/api/getresources";
        private static string postApiUrl(string prefix) => $"{baseServerUrl(prefix)}/api/postresources";

        private static string resourceUrlSection(ResourceType resourceType)
        {
            if (resourceType == ResourceType.Code) return "code";
            if (resourceType == ResourceType.Data) return "data";
            if (resourceType == ResourceType.Model) return "models";
            throw new NetworkUtilsException("Could not form request to server");
        }

        private static string resourceFileUrl(string prefix,ResourceType resourceType, string resourceName, string version)
        {
            return $"{getApiUrl(prefix)}/{resourceUrlSection(resourceType)}/{resourceName}/versions/{version}/file";
        }

        private static string resourceVersionsUrl(string prefix,ResourceType resourceType, string resourceName)
        {
            return $"{getApiUrl(prefix)}/{resourceUrlSection(resourceType)}/{resourceName}/versions";
        }

        private static string resourceDependenciesUrl(string prefix,ResourceType resourceType, string resourceName, string version)
        {
            return $"{getApiUrl(prefix)}/{resourceUrlSection(resourceType)}/{resourceName}/versions/{version}/dependencies";
        }

        private static string availableResourcesUrl(string prefix, ResourceType resourceType)
        {
            return $"{getApiUrl(prefix)}/{resourceUrlSection(resourceType)}";
        }

        private static string resourcePostUrl(string prefix,ResourceType resourceType, string resourceName, string version)
        {
            return $"{postApiUrl(prefix)}/{resourceUrlSection(resourceType)}/{resourceName}/versions/{version}/post";
        }

        private static readonly System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

        public static async System.Threading.Tasks.Task downloadResource(
            string prefix,
            ResourceType resourceType,
            string resourceName,
            string version,
            System.IO.Stream resultStream)
        {
            long totalFileSize;
            
            try
            {
                totalFileSize = getResourceVersions(prefix,resourceType, resourceName).versions[version].byteCount;
            }
            catch (System.Exception)
            {
                throw new NetworkUtilsException(
                    $"Could not find {resourceType.ToString().ToLower()} resource {resourceName} at version {version} on server"
                );
            }

            int bufferSize = System.Math.Max(1, System.Math.Min((int)(totalFileSize/100.0), 10000000));
            
            string url = resourceFileUrl(prefix,resourceType, resourceName, version);

            try
            {
                using (var contentStream = await client.GetStreamAsync(url))
                {
                    byte[] buffer = new byte[bufferSize];

                    bool doneReadingContent = false;

                    long totalBytesRead = 0;
                    
                    // @TODO add cancellation?
                    do
                    {
                        int bytesRead = contentStream.Read(buffer, 0, buffer.Length);
                        
                        if (bytesRead == 0)
                        {
                            doneReadingContent = true;
                        }
                        
                        totalBytesRead += bytesRead;

                        await resultStream.WriteAsync(buffer, 0, bytesRead);

                        int percentDone = totalFileSize == 0 ? 100 : (int)(100 * (double)totalBytesRead / (double)totalFileSize);

                        CLIInterface.writeBottomLineOverwriteExisting($"Download {resourceName}: {percentDone}%");
                    }
                    while(!doneReadingContent);
                    CLIInterface.logLine("");
                }
            }
            catch (System.Exception)
            {
                throw new NetworkUtilsException(
                    $"Unable to get file for {resourceType} resource {resourceName}"
                );
            }
        }

        public static ResourceVersionsInfoContainer getResourceVersions(string prefix,ResourceType resourceType, string resourceName)
        {
            string url = resourceVersionsUrl(prefix,resourceType, resourceName);

            string serializedInfo;
            try
            {
                serializedInfo = client.GetStringAsync(url).Result;
            }

            catch (System.Exception)
            {
                throw new NetworkUtilsException(
                    $"Unable to get list of versions of {resourceType} resource {resourceName} from server"
                );
            }
            try
            {
                ResourceVersionsInfoContainer versionsInfo = ResourceVersionsInfoContainer.deserialize(serializedInfo);

                return versionsInfo;
            }
            catch (System.Exception)
            {
                throw new NetworkUtilsException(
                    $"Unable to process server response to request for " +
                    $"list of versions of {resourceType} resource {resourceName}"
                );
            }
        }

        public static ResourceDependencyInfoContainer getResourceDependencies(string prefix,ResourceType resourceType, string resourceName, string version)
        {
            string url = resourceDependenciesUrl(prefix, resourceType, resourceName, version);

            string serializedInfo;

            try
            {
                serializedInfo = client.GetStringAsync(url).Result;
            }
            catch (System.Exception)
            {
                throw new NetworkUtilsException("Unable to get list of package dependencies from server");
            }
            
            try
            {
                ResourceDependencyInfoContainer dependencies = ResourceDependencyInfoContainer.deserialize(serializedInfo);
                return dependencies;
            }
            catch (System.Exception)
            {
                throw new NetworkUtilsException("Unable to process server response to request for list of dependencies");
            }
        }

        public static AvailableResourcesInfoContainer getAvailableResources(string prefix, ResourceType resourceType)
        {
            string url = availableResourcesUrl(prefix , resourceType);

            string serialized;
            try
            {
                serialized = client.GetStringAsync(url).Result;
            }
            catch (System.Exception)
            {
                throw new NetworkUtilsException("Unable to get list of available resources from server");
            }
            try
            {
                AvailableResourcesInfoContainer resources = AvailableResourcesInfoContainer.deserialize(serialized);
                return resources;
            }
            catch (System.Exception)
            {
                throw new NetworkUtilsException("Unable to process server response to available resources request");
            }
        }

        public static void publishResource(
            string prefix,
            System.IO.FileStream fileStream,
            ResourceType resourceType,
            string resourceName,
            string version,
            PublishDepsInfoContainer publishDepsInfo)
        {
            System.Collections.Specialized.NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

            queryString["deps"] = publishDepsInfo.serialize();

            string url = resourcePostUrl(prefix,resourceType, resourceName, version) + "?" + queryString.ToString();

            try
            {
                using (var fileContent = new System.Net.Http.StreamContent(fileStream))
                {
                    fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                    {
                        Name = $"files[{resourceName}]",
                        FileName = resourceName
                    };
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                    System.Net.Http.HttpResponseMessage statusResult = client.PostAsync(url, fileContent).Result;

                    if (!statusResult.IsSuccessStatusCode)
                    {
                        throw new NetworkUtilsException("Unable to publish: HTTP Error: " + statusResult.StatusCode.ToString());
                    }
                }
            }
            catch (NetworkUtilsException ex)
            {
                throw ex;
            }
            catch (System.Exception)
            {
                throw new NetworkUtilsException($"Unable to publish {resourceType} resource {resourceName} to server");
            }
        }
    }
}
