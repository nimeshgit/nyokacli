using System.Collections.Generic;
using Constants;
using FSOpsNS;
// using System.Linq;

namespace PackageManagerNS {
    public enum ResourceType { code, data, model }

    public static class PackageManager {
        public static void addPackage(ResourceType resourceType, string packageName) {
            FSOps.createCodeDataModelDirs();

            System.Console.WriteLine($"Adding {resourceType.ToString().ToLower()} resource \"{packageName}\"");
        }

        public static void listPackages(ResourceType? resourceType) {
            if (!FSOps.hasCodeDataModelDirs()) {
                System.Console.WriteLine($"Missing resource directories. Try running {ConstStrings.APPLICATION_ALIAS} init?");
                return;
            }

            List<ResourceType> resourcesToList = resourceType.HasValue ?
                new List<ResourceType> { resourceType.Value } :
                new List<ResourceType> { ResourceType.code, ResourceType.data, ResourceType.model };

            var resourceGetLists = new Dictionary<ResourceType, System.Func<IEnumerable<string>>>() {
                {ResourceType.code, () => FSOps.codeResourceNames()},
                {ResourceType.data, () => FSOps.dataResourceNames()},
                {ResourceType.model, () => FSOps.modelResourceNames()}
            };

            foreach (ResourceType resourceT in resourcesToList) {
                System.Console.WriteLine($"{resourceT.ToString()}:");
                foreach (string resourceName in resourceGetLists[resourceT]()) {
                    System.Console.WriteLine("    " + resourceName);
                }
            }
        }
    }
}