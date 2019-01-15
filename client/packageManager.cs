using System.Collections.Generic;
using Constants;
using FSOpsNS;
using NetworkUtilsNS;
using System.IO;
using System.Linq;
using InfoTransferContainers;
using CLIInterfaceNS;
using System.Threading.Tasks;

// @TODO add .nyoka file or something, with "nyoka remote https://server.org" to store information there
// @TODO rename client project to nyoka
// @TODO on server side, and on client side, prevent similar files with different capitalizations?
// @TODO Avoid windows reserved file names?
// @TODO make model plural, both on client side, server side, and in web api
// @TODO add possibility of making
// @TODO (later) make publish asdf.py the same as publish code/asdf.py
namespace PackageManagerNS
{
    public static class PackageManager
    {
        public class ResourceIdentifier
        {
            public string resourceName;
            public string version;
            public ResourceType resourceType;

            public ResourceIdentifier(string resourceName, ResourceType resourceType)
            {
                this.resourceName = resourceName;
                this.resourceType = resourceType;
                this.version = null;
            }

            public ResourceIdentifier(string resourceName, ResourceType resourceType, string version)
            {
                this.resourceName = resourceName;
                this.resourceType = resourceType;
                this.version = version;
            }
        }

        private static string bytesToString(long bytes)
        {
            const long KSize = 1024;
            const long MSize = 1048576;
            const long GSize = 1073741824;
            const long TSize = 1099511627776;

            long unit;
            string suffix;
            if (bytes < KSize)
            {
                unit = 1;
                suffix = "B";
            }
            else if (bytes < MSize)
            {
                unit = KSize;
                suffix = "KB";
            }
            else if (bytes < GSize)
            {
                unit = MSize;
                suffix = "MB";
            }
            else if (bytes < TSize)
            {
                unit = GSize;
                suffix = "GB";
            }
            else
            {
                unit = TSize;
                suffix = "TB";
            }

            float dividedByUnits = bytes/((float)unit);

            // represent either as integer or to two decimal places
            string numToString = dividedByUnits%1==0 ? dividedByUnits.ToString() : string.Format("{0:0.00}", dividedByUnits);

            return $"{numToString} {suffix}";
        }

        public static void initDirectories()
        {
            try
            {
                FSOps.createCodeDataModelDirs(logExisting: true, logCreated: true);
            }
            catch (FSOps.FSOpsException ex)
            {
                CLIInterface.logError($"File System Error: " + ex.Message);
            }
        }

        private static void downloadPackage(ResourceType resourceType, string resourceName, string version)
        {
            try
            {
                // check that resource is on server
                var availableResources = NetworkUtils.getAvailableResources(resourceType);
                if (!availableResources.resourceDescriptions.ContainsKey(resourceName))
                {
                    CLIInterface.logError($"Could not find {resourceType.ToString()} resource with name {resourceName} on server");
                    return;
                }

                // check that resource on server has specified version
                var versionInfo = NetworkUtils.getResourceVersions(resourceType, resourceName);
                if (!versionInfo.versions.Contains(version))
                {
                    CLIInterface.logError(
                        $"Could not find version {version} on server. These are the version(s) available: {string.Join(", ", versionInfo.versions)}"
                    );
                    return;
                }

                using (Stream resourceServerStream = NetworkUtils.getResource(resourceType, resourceName, version))
                using (FileStream resourceFileStream = FSOps.createResourceFile(resourceType, resourceName))
                using (StreamWriter versionFileStream = FSOps.createOrOverwriteResourceVersionFile(resourceType, resourceName))
                {
                    Task resourceFileTask = Task.Factory.StartNew(() => resourceServerStream.CopyTo(resourceFileStream));
                    Task resourceVersionTask = Task.Factory.StartNew(() => versionFileStream.WriteLine(version));

                    Task.WaitAll(resourceFileTask, resourceVersionTask);
                }
                CLIInterface.logLine($"{resourceType} resource {resourceName} added");
            }
            catch (NetworkUtils.NetworkUtilsException ex)
            {
                CLIInterface.logError($"Network Error: {ex.Message}");
            }
            catch (FSOps.FSOpsException ex)
            {
                CLIInterface.logError($"File System Error: " + ex.Message);
            }
        }

        public static void addPackage(ResourceIdentifier resourceDescription)
        {
            try
            {
                ResourceType resourceType = resourceDescription.resourceType;
                string resourceName = resourceDescription.resourceName;

                // check if the resource is available from the server
                var availableResources = NetworkUtils.getAvailableResources(resourceType);
                if (!availableResources.resourceDescriptions.ContainsKey(resourceName))
                {
                    CLIInterface.logError($"No resource called {resourceName} is available from the server.");
                    return;
                }

                string version = resourceDescription.version; // possible null
                var serverVersionInfo = NetworkUtils.getResourceVersions(resourceType, resourceName);

                if (version == null)
                {
                    version = serverVersionInfo.latestVersion;
                }
                else
                {
                    // check that the requested version is available from the server
                    if (!serverVersionInfo.versions.Contains(version))
                    {
                        CLIInterface.logError(
                            $"There is no version {version} available of resource {resourceName}. " +
                            $"These are the version(s) available: {string.Join(", ", serverVersionInfo.versions)}"
                        );
                        return;
                    }
                }

                // check if nyoka directories exists
                if (!FSOps.hasNecessaryDirs())
                {
                    bool createDirs = CLIInterface.askYesOrNo(
                        "Resource directories are not present in this directory. Create them now?"
                    );

                    if (createDirs)
                    {
                        FSOps.createCodeDataModelDirs(logCreated: true);
                    }
                    else
                    {
                        CLIInterface.logLine("Package add aborted");
                    }
                }

                // check if the resource is already present
                if (FSOps.resourceFileExists(resourceType, resourceName))
                {
                    bool continueAnyways = CLIInterface.askYesOrNo(
                        $"{resourceType.ToString()} resource {resourceName} is already present. Delete and replace ?"
                    );

                    if (continueAnyways)
                    {
                        FSOps.removeResourceFilesIfPresent(resourceType, resourceName);
                    }
                    else
                    {
                        CLIInterface.logLine("Aborting resource add.");
                        return;
                    }
                }

                ResourceDependencyInfoContainer dependencies = NetworkUtils.getResourceDependencies(resourceType, resourceName, version);

                var depDescriptions = new Dictionary<ResourceType, Dictionary<string, ResourceDependencyInfoContainer.DependencyDescription>> {
                    { ResourceType.Code, dependencies.codeDeps },
                    { ResourceType.Data, dependencies.dataDeps },
                    { ResourceType.Model, dependencies.modelDeps },
                };

                bool downloadDependencies = false;

                // if there package has any dependencies
                if (depDescriptions.Any(kvPair => kvPair.Value.Count != 0))
                {
                    CLIInterface.PrintTable table = new CLIInterface.PrintTable {
                        {"Resource Type", 13},
                        {"Dependency Type", 15},
                        {"Name of Resource", 15},
                        {"Resource Version", 15},
                        {"File Size", 10},
                    };

                    foreach (var (depResourceType, deps) in depDescriptions.Select(x => (x.Key, x.Value)))
                    {
                        foreach (var (depName, depDescription) in deps.Select(x => (x.Key, x.Value)))
                        {
                            table.addRow(
                                depResourceType.ToString(),
                                depDescription.isDirectDependency ? "direct" : "indirect",
                                depName,
                                depDescription.versionStr,
                                bytesToString(depDescription.byteCount)
                            );
                        }
                    }

                    CLIInterface.logLine($"Resource {resourceName} has these dependencies:");
                    CLIInterface.logTable(table);
                    downloadDependencies = CLIInterface.askYesOrNo("Download these dependencies?");

                    if (downloadDependencies) CLIInterface.logLine("Downloading dependencies");
                    else CLIInterface.logLine("Skipping downloading dependencies.");
                }

                if (downloadDependencies)
                {
                    foreach (var (depResourceType, deps) in depDescriptions.Select(x => (x.Key, x.Value)))
                    {
                        foreach (var (depName, depDescription) in deps.Select(x => (x.Key, x.Value)))
                        {
                            downloadPackage(depResourceType, depName, depDescription.versionStr);
                        }
                    }
                }

                CLIInterface.logLine($"Adding {resourceType.ToString()} resource \"{resourceName}\"");
                downloadPackage(resourceType, resourceName, version);
            }
            catch (FSOps.FSOpsException ex)
            {
                CLIInterface.logError($"File System Error: " + ex.Message);
            }
            catch (NetworkUtils.NetworkUtilsException ex)
            {
                CLIInterface.logError($"Network Error: {ex.Message}");
            }
        }

        public static void removePackage(ResourceIdentifier resourceDescription)
        {
            try
            {
                string resourceName = resourceDescription.resourceName;
                ResourceType resourceType = resourceDescription.resourceType;
                FSOps.createCodeDataModelDirs();

                CLIInterface.logLine($"Removing {resourceType.ToString().ToLower()} resource \"{resourceName}\"");
                if (!FSOps.resourceFileExists(resourceType, resourceName))
                {
                    CLIInterface.logError($"{resourceType} resource \"{resourceName}\" does not exist");
                    return;
                }

                if (resourceDescription.version != null)
                {
                    if (FSOps.resourceVersionFileExists(resourceType, resourceName))
                    {
                        string localVersion = FSOps.getResourceVersion(resourceType, resourceName);
                        if (localVersion != resourceDescription.version)
                        {
                            bool removeAnyways = CLIInterface.askYesOrNo($"Present version is {localVersion}, not {resourceDescription.version}. Remove anyways?");
                            if (!removeAnyways)
                            {
                                CLIInterface.logLine("Aborting resource removal.");
                                return;
                            }
                        }
                    }
                    else
                    {
                        bool removeAnyways = CLIInterface.askYesOrNo($"Local file {resourceName} has unknown version. Remove anyways?");
                        if (!removeAnyways)
                        {
                            CLIInterface.logLine("Aborting resource removal.");
                            return;
                        }
                    }
                }

                FSOps.removeResourceFilesIfPresent(resourceType, resourceName);
                CLIInterface.logLine("Resource removed");
            }
            catch (FSOps.FSOpsException ex)
            {
                CLIInterface.logError($"File System Error: " + ex.Message);
            }
        }

        public static void listResources(ResourceType? listType)
        {
            try
            {
                if (!FSOps.hasNecessaryDirs())
                {
                    CLIInterface.logError($"Missing some or all resource directories in current directory. Try running {ConstStrings.APPLICATION_ALIAS} init?");
                    return;
                }

                CLIInterface.PrintTable table = new CLIInterface.PrintTable {
                    {"Type", 7},
                    {"Name of Resource", 20},
                    {"Version", 15},
                    {"File Size", 10},
                };

                List<ResourceType> resourcesToList = listType.HasValue ?
                    new List<ResourceType> { listType.Value } :
                    new List<ResourceType> { ResourceType.Code, ResourceType.Data, ResourceType.Model };

                foreach (ResourceType resourceType in resourcesToList)
                {
                    foreach (string resourceName in FSOps.resourceNames(resourceType))
                    {
                        string version;
                        if (FSOps.resourceVersionFileExists(resourceType, resourceName))
                        {
                            version = FSOps.getResourceVersion(resourceType, resourceName);
                        }
                        else
                        {
                            version = "Unknown version";
                        }

                        long fileSize = FSOps.getResourceSize(resourceType, resourceName);

                        table.addRow(
                            resourceType.ToString(),
                            resourceName,
                            version,
                            bytesToString(fileSize)
                        );
                    }
                }

                CLIInterface.logTable(table);
            }
            catch (FSOps.FSOpsException ex)
            {
                CLIInterface.logError($"File System Error: " + ex.Message);
            }
        }

        public static void listDependencies(ResourceIdentifier resourceDescription)
        {
            string resourceName = resourceDescription.resourceName;
            ResourceType resourceType = resourceDescription.resourceType;
            string version = resourceDescription.version;
            
            try
            {
                // check if this resource exists on server
                var availableResources = NetworkUtils.getAvailableResources(resourceType);
                if (!availableResources.resourceDescriptions.ContainsKey(resourceName))
                {
                    CLIInterface.logError($"{resourceType.ToString()} resource {resourceName} could not be found on server");
                    return;
                }

                if (version == null)
                {
                    if (
                        FSOps.resourceFileExists(resourceType, resourceName)
                        && FSOps.resourceVersionFileExists(resourceType, resourceName)
                    )
                    {
                        version = FSOps.getResourceVersion(resourceType, resourceName);
                    }
                    else
                    {
                        var versionInfo = NetworkUtils.getResourceVersions(resourceType, resourceName);
                        version = versionInfo.latestVersion;
                    }
                }
                // check if user-specified version exists on the server at the given version
                else
                {
                    var versionInfo = NetworkUtils.getResourceVersions(resourceType, resourceName);
                    if (!versionInfo.versions.Contains(version))
                    {
                        CLIInterface.logError("Server does not report having a version \"{version}\" available for {resourceName}");
                    }
                }

                CLIInterface.logLine($"Showing dependencies of {resourceName}, version {version}");

                ResourceDependencyInfoContainer deps = NetworkUtils.getResourceDependencies(resourceType, resourceName, version);

                CLIInterface.PrintTable table = new CLIInterface.PrintTable {
                    {"Resource Type", 13},
                    {"Dependency Type", 15},
                    {"Name of Resource", 15},
                    {"Resource Version", 15},
                    {"File Size", 10},
                };

                var availableResourcesInfo = new Dictionary<ResourceType, AvailableResourcesInfoContainer> {
                    { ResourceType.Code, NetworkUtils.getAvailableResources(ResourceType.Code) },
                    { ResourceType.Data, NetworkUtils.getAvailableResources(ResourceType.Data) },
                    { ResourceType.Model, NetworkUtils.getAvailableResources(ResourceType.Model) },
                };

                var showDepDict = new Dictionary<ResourceType, Dictionary<string, ResourceDependencyInfoContainer.DependencyDescription>>() {
                    { ResourceType.Code, deps.codeDeps },
                    { ResourceType.Data, deps.dataDeps },
                    { ResourceType.Model, deps.modelDeps },
                };

                foreach (var (dependenciesType, descriptions) in showDepDict.Select(x => (x.Key, x.Value)))
                {
                    foreach (var (dependencyName, dependencyDescription) in descriptions.Select(x => (x.Key, x.Value)))
                    {
                        table.addRow(
                            dependenciesType.ToString(),
                            dependencyDescription.isDirectDependency ? "direct" : "indirect",
                            dependencyName,
                            dependencyDescription.versionStr,
                            bytesToString(availableResourcesInfo[dependenciesType].resourceDescriptions[dependencyName].byteCount)
                        );
                    }
                }

                CLIInterface.logTable(table);
            }
            catch (FSOps.FSOpsException ex)
            {
                CLIInterface.logError($"File System Error: " + ex.Message);
            }
            catch (NetworkUtils.NetworkUtilsException ex)
            {
                CLIInterface.logError($"Network Error: {ex.Message}");
            }
        }

        public static void listAvailableResources(ResourceType? listType)
        {
            try
            {
                List<ResourceType> resourcesToList = listType.HasValue ?
                    new List<ResourceType> { listType.Value } :
                    new List<ResourceType> { ResourceType.Code, ResourceType.Data, ResourceType.Model };

                CLIInterface.PrintTable printTable = new CLIInterface.PrintTable {
                    {"Type", 7},
                    {"Name of Resource", 20},
                    {"Latest Version", 15},
                    {"Local Version", 1},
                    {"File Size", 10},
                };

                foreach (ResourceType resourceType in resourcesToList)
                {
                    var availableResources = NetworkUtils.getAvailableResources(resourceType);

                    foreach (string resourceName in availableResources.resourceDescriptions.Keys.OrderBy(k => k))
                    {
                        string localVersionStr;
                        bool resourceExistsLocally = FSOps.resourceFileExists(resourceType, resourceName);
                        if (resourceExistsLocally)
                        {
                            if (FSOps.resourceVersionFileExists(resourceType, resourceName))
                            {
                                localVersionStr = FSOps.getResourceVersion(resourceType, resourceName);
                            }
                            else
                            {
                                localVersionStr = "Unknown version";
                            }
                        }
                        else
                        {
                            localVersionStr = "Not present";
                        }

                        printTable.addRow(
                            resourceType.ToString(),
                            resourceName,
                            availableResources.resourceDescriptions[resourceName].versionStr,
                            localVersionStr,
                            bytesToString(availableResources.resourceDescriptions[resourceName].byteCount)
                        );
                    }
                }

                CLIInterface.logTable(printTable);
            }
            catch (NetworkUtils.NetworkUtilsException ex)
            {
                CLIInterface.logError($"Network Error: {ex.Message}");
            }
            catch (FSOps.FSOpsException ex)
            {
                CLIInterface.logError($"File System Error: " + ex.Message);
            }
        }

        public static void publishResource(ResourceIdentifier resourceDescription, IEnumerable<ResourceIdentifier> deps)
        {
            try
            {
                PublishDepsInfoContainer publishDepsInfo = new PublishDepsInfoContainer();

                foreach (ResourceIdentifier depDescription in deps)
                {
                    if (depDescription.version == null)
                    {

                        CLIInterface.logError(
                            "The versions of dependencies must be supplied. For example, " +
                            "\"dependency.csv\" does not include version, \"dependency.csv@1.2.3\" does."
                        );
                        return;
                    }

                    var publishDepDescription = new PublishDepsInfoContainer.PublishDepDescription(depDescription.version);
                    if (depDescription.resourceType == ResourceType.Code)
                    {
                        publishDepsInfo.codeDeps[depDescription.resourceName] = publishDepDescription;
                    }
                    else if (depDescription.resourceType == ResourceType.Data)
                    {
                        publishDepsInfo.dataDeps[depDescription.resourceName] = publishDepDescription;
                    }
                    else if (depDescription.resourceType == ResourceType.Model)
                    {
                        publishDepsInfo.modelDeps[depDescription.resourceName] = publishDepDescription;
                    }
                }

                string resourceName = resourceDescription.resourceName;
                ResourceType resourceType = resourceDescription.resourceType;
                string publishVersion = resourceDescription.version;

                // check that user has provided version to publish file as
                if (publishVersion == null)
                {
                    publishVersion = "1.0";
                    CLIInterface.logLine($"Using default version {publishVersion}");
                }

                if (!FSOps.hasNecessaryDirs())
                {
                    CLIInterface.logError($"Could not find nyoka resource folders in current directory. Try running {ConstStrings.APPLICATION_ALIAS} {CLIParserNS.InitOptions.description.name}?");
                    return;
                }

                // If a file to publish with the given name can't be found
                if (!FSOps.resourceFileExists(resourceType, resourceName))
                {
                    CLIInterface.logError($"Resource with name {resourceName} not found.");
                    return;
                }

                var resourcesOnServer = NetworkUtils.getAvailableResources(resourceType);

                // If this resource already exists on server
                if (resourcesOnServer.resourceDescriptions.ContainsKey(resourceName))
                {
                    ResourceVersionsInfoContainer serverVersionsInfo = NetworkUtils.getResourceVersions(resourceType, resourceName);

                    // If this resource exists with the same version on server
                    if (serverVersionsInfo.versions.Contains(publishVersion))
                    {
                        bool continueAnyways = CLIInterface.askYesOrNo(
                            $"Version {publishVersion} of {resourceType.ToString()} resource " +
                            $"{resourceName} already exists on server. Overwrite?"
                        );

                        if (!continueAnyways)
                        {
                            CLIInterface.logLine("Aborting publish.");
                            return;
                        }
                        else
                        {
                            CLIInterface.logLine("Overwriting resource on server.");
                        }
                    }
                }

                CLIInterface.logLine("Opening file.");
                FileStream fileStream = FSOps.readResourceFile(resourceType, resourceName);

                CLIInterface.logLine("Uploading file.");
                NetworkUtils.publishResource(
                    fileStream,
                    resourceType,
                    resourceName,
                    publishVersion,
                    publishDepsInfo
                );

                // create or overwrite version file locally for this resource to be the publishVersion
                using (var versionFileStream = FSOps.createOrOverwriteResourceVersionFile(resourceType, resourceName))
                {
                    versionFileStream.WriteLine(publishVersion);
                }
            }
            catch (FSOps.FSOpsException ex)
            {
                CLIInterface.logError($"File System Error: " + ex.Message);
            }
            catch (NetworkUtils.NetworkUtilsException ex)
            {
                CLIInterface.logError($"Network Error: {ex.Message}");
            }
        }
    }
}
