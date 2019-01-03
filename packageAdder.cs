using System.Collections.Generic;

namespace PackageManagerNS {
    public enum ResourceType { Code, Data, Model }
    public static class PackageManager {
        public static void addPackage(ResourceType resourceType, string packageName) {
            FSOpsNS.FSOps.createCodeDataModelDirs();
            System.Console.WriteLine(resourceType);
            System.Console.WriteLine(packageName);
        }

        public static void listPackages(ResourceType? resourceType) {
            if (!resourceType.HasValue) {
                listCodePackages();
                listDataPackages();
                listModelPackages();
            } else if (resourceType.Value == ResourceType.Code) {
                listCodePackages();
            } else if (resourceType.Value == ResourceType.Data) {
                listDataPackages();
            } else if (resourceType.Value == ResourceType.Model) {
                listModelPackages();
            }
        }

        private static void listCodePackages() {
            System.Console.WriteLine("Code packages:");            
        }
        private static void listDataPackages() {
            System.Console.WriteLine("Data packages:");            
        }
        private static void listModelPackages() {
            System.Console.WriteLine("Model packages:");            
        }
    }
}