using System.Collections.Generic;
using Constants;
using FSOpsNS;
using NetworkUtilsNS;
using System.IO;
using System.Linq;
using InfoTransferContainers;
using FileTypeInferenceNS;
using CLIInterfaceNS;
using System.Threading.Tasks;

// @TODO make sure all streams are being closed properly?
namespace PackageManagerNS
{
    public static class PackageManager
    {
        private static ResourceType inferResourceType(string resourceName)
        {
            if (FileTypeInference.isCodeFileName(resourceName)) return ResourceType.code;
            if (FileTypeInference.isDataFileName(resourceName)) return ResourceType.data;
            if (FileTypeInference.isModelFileName(resourceName)) return ResourceType.model;
            throw new FileTypeInference.FileTypeInferenceError("Could not infer resource type from extension of {resourceName}");
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
                using (Stream resourceServerStream = NetworkUtils.getResource(resourceType, resourceName, version))
                using (FileStream resourceFileStream = FSOps.createResourceFile(resourceType, resourceName))
                using (StreamWriter versionFileStream = FSOps.createResourceFileNyokaVersionFile(resourceType, resourceName))
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
        
        public static void addPackage(string resourceName, string version)
        {
            try
            {
                ResourceType resourceType = inferResourceType(resourceName);

                // check if the resource is available from the server
                var availableResources = NetworkUtils.getAvailableResources(resourceType);
                if (!availableResources.ContainsKey(resourceName))
                {
                    CLIInterface.logError($"No resource called {resourceName} is available from the server.");
                    return;
                }

                // check if the requested version is available from the server
                var versionInfo = NetworkUtils.getResourceVersions(resourceType, resourceName);
                if (!versionInfo.versions.Contains(version))
                {
                    CLIInterface.logError($"There is no version {version} available of resource {resourceName}.");
                    return;
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
                if (FSOps.resourceExists(resourceType, resourceName))
                {
                    string presentVersion = FSOps.getResourceVersion(resourceType, resourceName);
                    
                    // if the resource is already present at the version requested
                    if (presentVersion == version)
                    {
                        CLIInterface.logLine($"{resourceType} resource \"{resourceName}\" is already present at version {version}");
                        return;
                    }
                    // if the resource is present, but at another version
                    else
                    {
                        bool continueAnyways = CLIInterface.askYesOrNo(
                            "{resourceType.ToString()} resource {resourceName} is already present" +
                            "at version {presentVersion}. Delete and replace with version {version}?"
                        );

                        if (continueAnyways)
                        {
                            FSOps.removeResource(resourceType, resourceName);
                        }
                        else
                        {
                            CLIInterface.logLine("Aborting resource add.");
                            return;
                        }
                    }
                }

                ResourceDependencyInfoContainer dependencies = NetworkUtils.getResourceDependencies(resourceType, resourceName, version);
                
                var depDescriptions = new Dictionary<ResourceType, Dictionary<string, ResourceDependencyInfoContainer.DependencyDescription>> {
                    { ResourceType.code, dependencies.codeDeps },
                    { ResourceType.data, dependencies.dataDeps },
                    { ResourceType.model, dependencies.modelDeps },
                };
                
                bool downloadDependencies = false;

                // if there package has any dependencies
                if (depDescriptions.Any(kvPair => kvPair.Value.Count != 0))
                {
                    CLIInterface.PrintTable table = new CLIInterface.PrintTable {
                        { "Resource Type", 13 },
                        { "Dep. Type", 9 },
                        { "Dependency Name", 20 },
                        { "Version", 15 },
                        { "Size", 10 },
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
            catch (FileTypeInference.FileTypeInferenceError ex)
            {
                CLIInterface.logError($"Argument Error: {ex.Message}");
            }
        }

        public static void removePackage(string resourceName)
        {
            try
            {
                ResourceType resourceType = inferResourceType(resourceName);
                FSOps.createCodeDataModelDirs();

                CLIInterface.logLine($"Removing {resourceType.ToString().ToLower()} resource \"{resourceName}\"");
                if (!FSOps.resourceExists(resourceType, resourceName))
                {
                    CLIInterface.logError($"{resourceType} resource \"{resourceName}\" does not exist");
                    return;
                }

                FSOps.removeResource(resourceType, resourceName);
                CLIInterface.logLine("Resource removed");
            }
            catch (FSOps.FSOpsException ex)
            {
                CLIInterface.logError($"File System Error: " + ex.Message);
            }
            catch (FileTypeInference.FileTypeInferenceError ex)
            {
                CLIInterface.logError($"Argument Error: {ex.Message}");
            }
        }

        public static void listResources(ResourceType? listType)
        {
            try
            {
                if (!FSOps.hasNecessaryDirs())
                {
                    CLIInterface.logError($"Missing resource directories or files. Try running {ConstStrings.APPLICATION_ALIAS} init?");
                    return;
                }

                CLIInterface.PrintTable table = new CLIInterface.PrintTable {
                    {"Type", 7},
                    {"Name of Resource", 20},
                    {"Version", 15},
                    {"Size", 10},
                };

                List<ResourceType> resourcesToList = listType.HasValue ?
                    new List<ResourceType> { listType.Value } :
                    new List<ResourceType> { ResourceType.code, ResourceType.data, ResourceType.model };

                foreach (ResourceType resourceType in resourcesToList)
                {
                    foreach (string resourceName in FSOps.resourceNames(resourceType))
                    {
                        string version = FSOps.getResourceVersion(resourceType, resourceName);
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

        public static void listDependencies(string resourceName, string version)
        {
            try
            {
                ResourceType resourceType = inferResourceType(resourceName);
                
                if (version == null)
                {
                    if (FSOps.resourceExists(resourceType, resourceName))
                    {
                        version = FSOps.getResourceVersion(resourceType, resourceName);
                    }
                    else
                    {
                        CLIInterface.logError("Provide version number for resources not currently downloaded");
                        return;
                    }
                }
                
                ResourceDependencyInfoContainer deps = NetworkUtils.getResourceDependencies(resourceType, resourceName, version);

                CLIInterface.PrintTable table = new CLIInterface.PrintTable {
                    {"Resource Type", 13},
                    {"Dep. Type", 9},
                    {"Dependency Name", 20},
                    {"Version", 15},
                    {"Size", 10},
                };

                var availableResourcesInfo = new Dictionary<ResourceType, Dictionary<string, FileInfoTransferContainer>> {
                    { ResourceType.code, NetworkUtils.getAvailableResources(ResourceType.code) },
                    { ResourceType.data, NetworkUtils.getAvailableResources(ResourceType.data) },
                    { ResourceType.model, NetworkUtils.getAvailableResources(ResourceType.model) },
                };

                var showDepDict = new Dictionary<ResourceType, Dictionary<string, ResourceDependencyInfoContainer.DependencyDescription>>() {
                    { ResourceType.code, deps.codeDeps },
                    { ResourceType.data, deps.dataDeps },
                    { ResourceType.model, deps.modelDeps },
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
                            bytesToString(availableResourcesInfo[dependenciesType][dependencyName].byteCount)
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
            catch (FileTypeInference.FileTypeInferenceError ex)
            {
                CLIInterface.logError($"Argument Error: {ex.Message}");
            }
        }

        public static void listAvailableResources(ResourceType? listType)
        {
            try
            {
                List<ResourceType> resourcesToList = listType.HasValue ?
                    new List<ResourceType> { listType.Value } :
                    new List<ResourceType> { ResourceType.code, ResourceType.data, ResourceType.model };
                
                CLIInterface.PrintTable printTable = new CLIInterface.PrintTable {
                    {"Type", 7},
                    {"Name of Resource", 20},
                    {"Latest Version", 15},
                    {"Local Version", 1},
                    {"Size", 10},
                };

                foreach (ResourceType resourceType in resourcesToList)
                {
                    var availableResources = NetworkUtils.getAvailableResources(resourceType);
                    
                    foreach (string resourceName in availableResources.Keys.OrderBy(k => k))
                    {
                        bool resourceExistsLocally = FSOps.resourceExists(resourceType, resourceName);
                        printTable.addRow(
                            resourceType.ToString(),
                            resourceName,
                            availableResources[resourceName].versionStr,
                            resourceExistsLocally ? FSOps.getResourceVersion(resourceType, resourceName) : "Not Downloaded" ,
                            bytesToString(availableResources[resourceName].byteCount)
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

        private static (ResourceType, string) nextResourceToInvestigate(Dictionary<ResourceType, List<string>> investigateResourcesByType)
        {
            foreach (var (resourceType, uninvestigatedNames) in investigateResourcesByType.Select(x => (x.Key, x.Value)))
            {
                if (uninvestigatedNames.Count == 0) continue;
                
                string resourceName = uninvestigatedNames.Last();
                uninvestigatedNames.RemoveAt(uninvestigatedNames.Count - 1);
                
                return (resourceType, resourceName);
            }
            
            throw new System.InvalidOperationException(); // @QUESTION is this an appropriate exception?
        }

        private static void addDependenciesToInvestigateDict(Dictionary<ResourceType, List<string>> investigateResourcesByType, ResourceDependencyInfoContainer infoContainer)
        {
            var dependencyDescriptionsByResourceType = new Dictionary<ResourceType, Dictionary<string, ResourceDependencyInfoContainer.DependencyDescription>> {
                { ResourceType.code, infoContainer.codeDeps },
                { ResourceType.data, infoContainer.dataDeps },
                { ResourceType.model, infoContainer.modelDeps },
            };

            foreach (var (resourceType, dependencyDescriptionsDict) in dependencyDescriptionsByResourceType.Select(x => (x.Key, x.Value)))
            {
                foreach (string resourceName in dependencyDescriptionsDict.Keys)
                {
                    if (!investigateResourcesByType[resourceType].Contains(resourceName))
                    {
                        investigateResourcesByType[resourceType].Add(resourceName);
                    }
                }
            }
        }

        public static void pruneTo(List<string> pruneToCode, List<string> pruneToData, List<string> pruneToModel)
        {
            try
            {
                var pruneToArgs = new Dictionary<ResourceType, List<string>> {
                    { ResourceType.code, pruneToCode },
                    { ResourceType.data, pruneToData },
                    { ResourceType.model, pruneToModel },
                };
                var resourcesToKeep = new Dictionary<ResourceType, List<string>> {
                    { ResourceType.code, pruneToCode },
                    { ResourceType.data, pruneToData },
                    { ResourceType.model, pruneToModel },
                };

                foreach (var (pruneResourceType, pruneToNames) in pruneToArgs.Select(x => (x.Key, x.Value)))
                {
                    foreach (string pruneToName in pruneToNames)
                    {
                        if (!FSOps.resourceExists(pruneResourceType, pruneToName))
                        {
                            CLIInterface.logLine($"Skipping missing {pruneResourceType.ToString()} resource {pruneToName}");
                            continue;
                        }
                        
                        string version = FSOps.getResourceVersion(pruneResourceType, pruneToName);

                        ResourceDependencyInfoContainer infoContainer = NetworkUtils.getResourceDependencies(pruneResourceType, pruneToName, version);

                        var mergeDependencies = new Dictionary<ResourceType, Dictionary<string, ResourceDependencyInfoContainer.DependencyDescription>> {
                            { ResourceType.code, infoContainer.codeDeps },
                            { ResourceType.data, infoContainer.dataDeps },
                            { ResourceType.model, infoContainer.modelDeps },
                        };

                        foreach (var (mergeDepResourceType, mergeDepDict) in mergeDependencies.Select(x => (x.Key, x.Value)))
                        {
                            // @TODO take into account versions?
                            foreach (var (mergeDepName, mergeDepDescription) in mergeDepDict.Select(x => (x.Key, x.Value)))
                            {
                                if (!resourcesToKeep[mergeDepResourceType].Contains(mergeDepName))
                                {
                                    resourcesToKeep[mergeDepResourceType].Add(mergeDepName);
                                }
                            }
                        }
                    }
                }
                

                var presentResources = new Dictionary<ResourceType, IEnumerable<string>> {
                    { ResourceType.code, FSOps.resourceNames(ResourceType.code) },
                    { ResourceType.data, FSOps.resourceNames(ResourceType.data) },
                    { ResourceType.model, FSOps.resourceNames(ResourceType.model) },
                };

                bool successful = true;
                foreach (var (resourceType, presentResourceNameList) in presentResources.Select(x => (x.Key, x.Value)))
                {
                    foreach (string presentResourceName in presentResourceNameList)
                    {
                        if (resourcesToKeep[resourceType].Contains(presentResourceName))
                        {
                            CLIInterface.logLine($"Keeping {resourceType.ToString()} resource \"{presentResourceName}\"");
                        }
                        else
                        {
                            FSOps.removeResource(resourceType, presentResourceName);
                            CLIInterface.logLine($"Removed {resourceType.ToString()} resource {presentResourceName}");
                        }
                    }
                }

                if (!successful)
                {
                    CLIInterface.logError("Failed to remove some resources.");
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
        
        public static void publishResource(
            string resourceName,
            string publishVersion,
            List<string> codeDeps,
            List<string> dataDeps,
            List<string> modelDeps)
        {
            try
            {
                ResourceType resourceType = inferResourceType(resourceName);
                
                PublishDepsInfoContainer publishDepsInfo = new PublishDepsInfoContainer();

                (Dictionary<string, PublishDepsInfoContainer.PublishDepDescription>, List<string>)[] publishParsePairs = {
                    (publishDepsInfo.codeDeps, codeDeps),
                    (publishDepsInfo.dataDeps, dataDeps),
                    (publishDepsInfo.modelDeps, modelDeps),
                };

                foreach (var (publishDepDict, resourceStringList) in publishParsePairs)
                {
                    foreach (string resourceString in resourceStringList)
                    {
                        string[] splitByAtSign = resourceString.Split("@");
                        
                        if (splitByAtSign.Length != 2)
                        {
                            CLIInterface.logError($"Invalid resource name {resourceString}. It should be formatted like this: \"[resource name]@[version number]\"");
                            return;
                        }
                        string depName = splitByAtSign[0];
                        string depVersion = splitByAtSign[1];
                        
                        publishDepDict[depName] = new PublishDepsInfoContainer.PublishDepDescription(depVersion);
                    }
                }

                var allAvailableResources = new Dictionary<ResourceType, Dictionary<string, FileInfoTransferContainer>> {
                    { ResourceType.code, NetworkUtils.getAvailableResources(ResourceType.code) },
                    { ResourceType.data, NetworkUtils.getAvailableResources(ResourceType.data) },
                    { ResourceType.model, NetworkUtils.getAvailableResources(ResourceType.model) },
                };

                // If a file to publish with the given name can't be found
                if (!FSOps.checkPublishFileExists(resourceName))
                {
                    CLIInterface.logError($"Resource with name {resourceName} not found in current directory.");
                    return;
                }

                // If this resource exists on server
                if (allAvailableResources[resourceType].ContainsKey(resourceName))
                {
                    ResourceVersionsInfoContainer versionsInfo = NetworkUtils.getResourceVersions(resourceType, resourceName);

                    // If this resource exists with the same version on server
                    if (versionsInfo.versions.Contains(publishVersion))
                    {
                        bool continueAnyways = CLIInterface.askYesOrNo(
                            "Version {publishVersion} of {resourceType.ToString()} resource" +
                            "{resourceName} already exists on server. Overwrite?"
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
                FileStream fileStream = FSOps.readPublishFile(resourceName);
                
                CLIInterface.logLine("Uploading file.");
                NetworkUtils.publishResource(
                    fileStream,
                    resourceType,
                    resourceName,
                    publishVersion,
                    publishDepsInfo
                );
            }
            catch (FSOps.FSOpsException ex)
            {
                CLIInterface.logError($"File System Error: " + ex.Message);
            }
            catch (NetworkUtils.NetworkUtilsException ex)
            {
                CLIInterface.logError($"Network Error: {ex.Message}");
            }
            catch (FileTypeInference.FileTypeInferenceError ex)
            {
                CLIInterface.logError($"Argument Error: {ex.Message}");
            }
        }
    }
}