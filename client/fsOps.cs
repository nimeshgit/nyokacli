using System.Collections.Generic;
using System.Linq;
using System.IO;
using Constants;

namespace FSOpsNS {
    public static class FSOps {
        private static string codeDirName = "code";
        private static string dataDirName = "data";
        private static string modelDirName = "model";
        private static string[] dirNames = new string[] { codeDirName, dataDirName, modelDirName };

        public static List<string> a() {
            return new List<string>();
        }
        
        public static bool hasCodeDataModelDirs() {
            foreach (string dirNames in FSOps.dirNames) {
                if (!Directory.Exists(dirNames)) {
                    return false;
                }
            }

            return true;
        }
        
        public static bool createCodeDataModelDirs(bool logExisting = false, bool logCreated = false, bool logError = true) {
            bool successful = true;
            
            foreach (string dirName in FSOps.dirNames) {
                if (Directory.Exists(dirName)) {
                    if (logExisting) {
                        System.Console.WriteLine($"Directory \"{dirName}\" already exists");
                    }
                } else {
                    try {
                        Directory.CreateDirectory(dirName);
                        
                        if (logCreated) {
                            System.Console.WriteLine($"Directory \"{dirName}\" created");
                        }
                    } catch (IOException) {
                        if (logError) {
                            System.Console.WriteLine($"Failed to create directory \"{dirName}\"");
                        }
                        successful = false;
                    }
                }
            }

            return successful;
        }

        public static void removeResource(ResourceType resourceType, string resourceName) {
            File.Delete($"{resourceType.ToString()}/{resourceName}");
        }
        
        public static bool resourceExists(ResourceType resourceType, string resourceName) {
            return File.Exists($"{resourceType.ToString()}/{resourceName}");
        }

        public static IEnumerable<string> resourceNames(ResourceType resourceType) {
            return new DirectoryInfo(resourceType.ToString()).EnumerateFiles().Select(file => file.Name);
        }

        public static FileStream createResourceFile(ResourceType resourceType, string resourceName) {
            return File.Create($"{resourceType.ToString()}/{resourceName}");
        }
    }
}