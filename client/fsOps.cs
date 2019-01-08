using System.Collections.Generic;
using System.Linq;
using System.IO;
using Constants;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CLIInterfaceNS;

namespace FSOpsNS
{
    public static class FSOps
    {
        private static readonly string codeDirName = "code";
        private static readonly string dataDirName = "data";
        private static readonly string modelDirName = "model";
        private static readonly string nyokaFolderName = ".nyoka";

        private static readonly string nyokaVersionExtension = ".version";
        private static readonly string[] dirNames = new string[] { codeDirName, dataDirName, modelDirName };
        
        public static bool hasNecessaryDirsAndFiles()
        {
            foreach (string dirName in dirNames)
            {
                if (!Directory.Exists(dirName))
                {
                    return false;
                }
                
                if (!Directory.Exists(Path.Join(dirName, nyokaFolderName)))
                {
                    return false;
                }
            }

            return true;
        }
        
        public static bool createCodeDataModelDirs(
            bool logExisting = false,
            bool logCreated = false,
            bool logError = true)
        {
            bool successful = true;
            
            foreach (string dirName in dirNames)
            {
                tryCreateDirIfNonExistent(
                    dirName,
                    logExisting,
                    logCreated,
                    logError
                );

                tryCreateDirIfNonExistent(
                    Path.Join(dirName, nyokaFolderName),
                    logExisting,
                    logCreated,
                    logError
                );
            }

            return successful;
        }

        private static bool tryCreateDirIfNonExistent(
            string dirName,
            bool logExisting,
            bool logCreated,
            bool logError)
        {
            if (Directory.Exists(dirName))
            {
                if (logExisting)
                {
                    CLIInterface.log($"Directory \"{dirName}\" already exists");
                }
                return true;
            } 
            else
            {
                try
                {
                    Directory.CreateDirectory(dirName);
                    
                    if (logCreated)
                    {
                        CLIInterface.log($"Directory \"{dirName}\" created");
                    }
                    return true;
                }
                catch (IOException)
                {
                    if (logError)
                    {
                        CLIInterface.log($"Failed to create directory \"{dirName}\"");
                    }
                    return false;
                }
            }
        }

        public static void removeResource(ResourceType resourceType, string resourceName)
        {
            File.Delete(Path.Join(resourceType.ToString(), resourceName));
            File.Delete(Path.Join(resourceType.ToString(), nyokaFolderName, resourceName + nyokaVersionExtension));
        }
        
        public static bool resourceExists(ResourceType resourceType, string resourceName)
        {
            return File.Exists(Path.Join(resourceType.ToString(), resourceName));
        }

        public static IEnumerable<string> resourceNames(ResourceType resourceType)
        {
            return new DirectoryInfo(resourceType.ToString()).EnumerateFiles().Select(file => file.Name);
        }

        public static FileStream createResourceFile(ResourceType resourceType, string resourceName)
        {
            return File.Create(Path.Join(resourceType.ToString(), resourceName));
        }

        public static StreamWriter createResourceFileNyokaVersionFile(ResourceType resourceType, string resourceName)
        {
            return File.CreateText(Path.Join(resourceType.ToString(), nyokaFolderName, resourceName + nyokaVersionExtension));
        }

        public static string getResourceVersion(ResourceType resourceType, string resourceName)
        {
            return File.ReadAllText(Path.Join(resourceType.ToString(), nyokaFolderName, resourceName + nyokaVersionExtension)).Trim();
        }

        public static long getResourceSize(ResourceType resourceType, string resourceName)
        {
            return new FileInfo(Path.Join(resourceType.ToString(), resourceName)).Length;
        }
    }
}