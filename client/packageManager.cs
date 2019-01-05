using System.Collections.Generic;
using Constants;
using FSOpsNS;
using NetworkUtilsNS;
using System.IO;
using System.Linq;
using InfoTransferContainers;
using TablePrinterNS;

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
        
        public static void addPackage(ResourceType resourceType, string resourceName)
        {
            FSOps.createCodeDataModelDirs();
            System.Console.WriteLine($"Adding {resourceType.ToString().ToLower()} resource \"{resourceName}\"");
            
            if (FSOps.resourceExists(resourceType, resourceName))
            {
                System.Console.WriteLine($"{resourceType} resource \"{resourceName}\" already exists");
                return;
            }

            try
            {
                using (Stream resourceStream = NetworkUtils.getResource(resourceType, resourceName))
                using (FileStream fileStream = FSOps.createResourceFile(resourceType, resourceName))
                {
                    resourceStream.CopyTo(fileStream);
                }
                System.Console.WriteLine("Resource added");
            }
            catch (NetworkUtils.NetworkUtilsException e)
            {
                System.Console.WriteLine("Network error: " + e.Message);
            }
        }

        public static void removePackage(ResourceType resourceType, string resourceName)
        {
            FSOps.createCodeDataModelDirs();

            System.Console.WriteLine($"Removing {resourceType.ToString().ToLower()} resource \"{resourceName}\"");
            if (!FSOps.resourceExists(resourceType, resourceName))
            {
                System.Console.WriteLine($"{resourceType} resource \"{resourceName}\" does not exist");
                return;
            }

            FSOps.removeResource(resourceType, resourceName);
            System.Console.WriteLine("Resource removed");
        }

        public static void listResources(ResourceType? listType)
        {
            if (!FSOps.hasNecessaryDirsAndFiles())
            {
                System.Console.WriteLine($"Missing resource directories or files. Try running {ConstStrings.APPLICATION_ALIAS} init?");
                return;
            }

            TablePrinter tablePrinter = new TablePrinter {
                {"Type", 7},
                {"Name of Resource", 30},
                {"Version", 15},
            };

            List<ResourceType> resourcesToList = listType.HasValue ?
                new List<ResourceType> { listType.Value } :
                new List<ResourceType> { ResourceType.code, ResourceType.data, ResourceType.model };

            foreach (ResourceType resourceType in resourcesToList)
            {
                foreach (string resourceName in FSOps.resourceNames(resourceType))
                {
                    tablePrinter.addLine(resourceType.ToString(), resourceName, "???");
                }
            }

            tablePrinter.print();
        }

        public static void listDependencies(ResourceType resourceType, string resourceName, string version)
        {
            DepsTransferContainer deps;
            try {
                deps= NetworkUtils.getResourceDeps(resourceType, resourceName, version);
            }
            catch (NetworkUtils.NetworkUtilsException ex)
            {
                System.Console.WriteLine("Network error: " + ex.Message);
                return;
            }

            TablePrinter tablePrinter = new TablePrinter {
                {"Type", 7},
                {"Name Of Dependency", 30},
                {"Version", 15},
            };

            var showDepDict = new Dictionary<string, Dictionary<string, DepsTransferContainer.DepDescription>>() {
                {"Code", deps.codeDeps},
                {"Data", deps.dataDeps},
                {"Model", deps.modelDeps}
            };
            
            foreach (var typeEntry in showDepDict) {
                foreach (var dependencyEntry in typeEntry.Value) {
                    DepsTransferContainer.DepDescription dependencyDescription = dependencyEntry.Value;

                    tablePrinter.addLine(typeEntry.Key, dependencyEntry.Key, dependencyDescription.versionStr);
                }
            }

            tablePrinter.print();
        }

        public static void listAvailableResources(ResourceType? listType)
        {
            List<ResourceType> resourcesToList = listType.HasValue ?
                new List<ResourceType> { listType.Value } :
                new List<ResourceType> { ResourceType.code, ResourceType.data, ResourceType.model };
            
            try
            {
                TablePrinter tablePrinter = new TablePrinter {
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
                        tablePrinter.addLine(
                            resourceType.ToString(),
                            resourceName,
                            availableResources[resourceName].versionStr,
                            bytesToString(availableResources[resourceName].byteCount)
                        );
                    }
                }

                tablePrinter.print();
            }
            catch (NetworkUtils.NetworkUtilsException ex)
            {
                System.Console.WriteLine("Network error: " + ex.Message);
            }
        }
    }
}