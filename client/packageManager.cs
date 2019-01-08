using System.Collections.Generic;
using Constants;
using FSOpsNS;
using NetworkUtilsNS;
using System.IO;
using System.Linq;
using InfoTransferContainers;
using CLIInterfaceNS;
using System.Threading.Tasks;

// @TODO make sure all streams are being closed properly?
namespace PackageManagerNS
{
    public static class PackageManager
    {
        private static string bytesToString(long bytes) {
            const long KSize = 1024;
            const long MSize = 1048576;
            const long GSize = 1073741824;
            const long TSize = 1099511627776;	

            long unit;
            string suffix;
            if (bytes < KSize) {
                unit = 1;
                suffix = "B";
            } else if (bytes < MSize) {
                unit = KSize;
                suffix = "KB";
            } else if (bytes < GSize) {
                unit = MSize;
                suffix = "MB";
            } else if (bytes < TSize) {
                unit = GSize;
                suffix = "GB";
            } else {
                unit = TSize;
                suffix = "TB";
            }

            float dividedByUnits = bytes/((float)unit);
            string numToString = dividedByUnits%1==0 ? dividedByUnits.ToString() : string.Format("{0:0.00}", dividedByUnits);

            return $"{numToString} {suffix}";
        }
        
        public static void initDirectories()
        {
            FSOps.createCodeDataModelDirs(logExisting: true, logCreated: true, logError: true);
        }
        
        public static void addPackage(ResourceType resourceType, string resourceName, string version)
        {
            FSOps.createCodeDataModelDirs();
            CLIInterface.log($"Adding {resourceType.ToString().ToLower()} resource \"{resourceName}\"");
            
            if (FSOps.resourceExists(resourceType, resourceName))
            {
                CLIInterface.log($"{resourceType} resource \"{resourceName}\" already exists");
                return;
            }

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
                CLIInterface.log("Resource added");
            }
            catch (NetworkUtils.NetworkUtilsException e)
            {
                CLIInterface.logError("Network error: " + e.Message);
            }
        }

        public static void removePackage(ResourceType resourceType, string resourceName)
        {
            FSOps.createCodeDataModelDirs();

            CLIInterface.log($"Removing {resourceType.ToString().ToLower()} resource \"{resourceName}\"");
            if (!FSOps.resourceExists(resourceType, resourceName))
            {
                CLIInterface.logError($"{resourceType} resource \"{resourceName}\" does not exist");
                return;
            }

            FSOps.removeResource(resourceType, resourceName);
            CLIInterface.log("Resource removed");
        }

        public static void listResources(ResourceType? listType)
        {
            if (!FSOps.hasNecessaryDirsAndFiles())
            {
                CLIInterface.logError($"Missing resource directories or files. Try running {ConstStrings.APPLICATION_ALIAS} init?");
                return;
            }

            CLIInterface.PrintTable table = new CLIInterface.PrintTable {
                {"Type", 7},
                {"Name of Resource", 30},
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
                    
                    table.addLine(
                        resourceType.ToString(),
                        resourceName,
                        version,
                        bytesToString(fileSize)
                    );
                }
            }
            
            CLIInterface.log(table);
        }

        public static void listDependencies(ResourceType resourceType, string resourceName, string version)
        {
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
            
            ResourceDependencyInfoContainer deps;
            try {
                deps = NetworkUtils.getResourceInfo(resourceType, resourceName, version);
            }
            catch (NetworkUtils.NetworkUtilsException ex)
            {
                CLIInterface.logError("Network error: " + ex.Message);
                return;
            }

            CLIInterface.PrintTable table = new CLIInterface.PrintTable {
                {"Type", 7},
                {"Name Of Dependency", 30},
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
                    table.addLine(
                        dependenciesType.ToString(),
                        dependencyName,
                        dependencyDescription.versionStr,
                        bytesToString(availableResourcesInfo[dependenciesType][dependencyName].byteCount)
                    );
                }
            }
            
            CLIInterface.log(table);
        }

        public static void listAvailableResources(ResourceType? listType)
        {
            List<ResourceType> resourcesToList = listType.HasValue ?
                new List<ResourceType> { listType.Value } :
                new List<ResourceType> { ResourceType.code, ResourceType.data, ResourceType.model };
            
            try
            {
                CLIInterface.PrintTable printTable = new CLIInterface.PrintTable {
                    {"Type", 7},
                    {"Name of Resource", 30},
                    {"Latest Version", 15},
                    {"Size", 10},
                };

                foreach (ResourceType resourceType in resourcesToList)
                {
                    var availableResources = NetworkUtils.getAvailableResources(resourceType);
                    
                    foreach (string resourceName in availableResources.Keys.OrderBy(k => k))
                    {
                        printTable.addLine(
                            resourceType.ToString(),
                            resourceName,
                            availableResources[resourceName].versionStr,
                            bytesToString(availableResources[resourceName].byteCount)
                        );
                    }
                }
                
                CLIInterface.log(printTable);
            }
            catch (NetworkUtils.NetworkUtilsException ex)
            {
                CLIInterface.logError("Network error: " + ex.Message);
            }
        }

        private static ResourceDependencyInfoContainer infoForLocalResource(ResourceType resourceType, string resourceName)
        {
            string version = FSOps.getResourceVersion(resourceType, resourceName);
            return NetworkUtils.getResourceInfo(resourceType, resourceName, version);
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
            var resourcesToKeep = new Dictionary<ResourceType, List<string>> {
                { ResourceType.code, new List<string>() },
                { ResourceType.data, new List<string>() },
                { ResourceType.model, new List<string>() },
            };
            
            var resourcesToInvestigateByType = new Dictionary<ResourceType, List<string>> {
                { ResourceType.code, pruneToCode },
                { ResourceType.data, pruneToData },
                { ResourceType.model, pruneToModel },
            };

            while (
                // There are resources left to investigate
                resourcesToInvestigateByType.Any(entry => entry.Value.Count != 0)
            ) {
                var (investigateResourceType, investigateResourceName) = nextResourceToInvestigate(resourcesToInvestigateByType);

                if (!FSOps.resourceExists(investigateResourceType, investigateResourceName))
                {
                    CLIInterface.logWarning($"Skipping missing {investigateResourceType.ToString()} resource \"{investigateResourceName}\"");
                    continue;
                }
                else
                {
                    resourcesToKeep[investigateResourceType].Add(investigateResourceName);
                }
                
                string version = FSOps.getResourceVersion(investigateResourceType, investigateResourceName);

                ResourceDependencyInfoContainer resourceInfo = NetworkUtils.getResourceInfo(investigateResourceType, investigateResourceName, version);
                
                addDependenciesToInvestigateDict(resourcesToInvestigateByType, resourceInfo);
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
                        CLIInterface.log($"Keeping {resourceType.ToString()} resource \"{presentResourceName}\"");
                    }
                    else
                    {
                        try
                        {
                            FSOps.removeResource(resourceType, presentResourceName);
                            CLIInterface.log($"Removed {resourceType.ToString()} resource {presentResourceName}");
                        }
                        catch (System.Exception)
                        {
                            CLIInterface.logError($"Failed to remove {resourceType.ToString()} resource {presentResourceName}");
                            successful = false;
                        }
                    }
                }
            }

            if (!successful)
            {
                CLIInterface.logError("Failed to remove some packages.");
            }
        }

        public static void publishResource(
            ResourceType resourceType,
            string resourceName,
            string resourceVersion,
            List<string> codeDeps,
            List<string> dataDeps,
            List<string> modelDeps)
        {
            var allAvailableResources = new Dictionary<ResourceType, Dictionary<string, FileInfoTransferContainer>> {
                { ResourceType.code, NetworkUtils.getAvailableResources(ResourceType.code) },
                { ResourceType.data, NetworkUtils.getAvailableResources(ResourceType.data) },
                { ResourceType.model, NetworkUtils.getAvailableResources(ResourceType.model) },
            };

            if (allAvailableResources[resourceType].ContainsKey(resourceName))
            {
            }
        }
    }
}