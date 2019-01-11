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
        public class FSOpsException : System.Exception {
            public FSOpsException(string message)
            : base(message)
            {
            }
        }
        private static readonly string codeDirName = "code";
        private static readonly string dataDirName = "data";
        private static readonly string modelDirName = "model";
        private static readonly string nyokaFolderName = ".nyoka";

        private static readonly string nyokaVersionExtension = ".version";
        private static readonly string[] dirNames = new string[] { codeDirName, dataDirName, modelDirName };
        
        public static bool hasNecessaryDirs()
        {
            try
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
            catch (System.Exception)
            {
                throw new FSOpsException("Failed to check for necessary directories and files");
            }
        }
        
        public static bool createCodeDataModelDirs(
            bool logExisting = false,
            bool logCreated = false)
        {
            try
            {
                bool successful = true;
                
                foreach (string dirName in dirNames)
                {
                    tryCreateDirIfNonExistent(
                        dirName,
                        logExisting,
                        logCreated
                    );

                    tryCreateDirIfNonExistent(
                        Path.Join(dirName, nyokaFolderName),
                        logExisting,
                        logCreated
                    );
                }

                return successful;
            }
            catch (FSOpsException ex)
            {
                throw ex;
            }
        }

        private static bool tryCreateDirIfNonExistent(
            string dirName,
            bool logExisting,
            bool logCreated)
        {
            try
            {
                if (Directory.Exists(dirName))
                {
                    if (logExisting)
                    {
                        CLIInterface.logLine($"Directory \"{dirName}\" already exists");
                    }
                    return true;
                } 
                else
                {
                    Directory.CreateDirectory(dirName);
                    
                    if (logCreated)
                    {
                        CLIInterface.logLine($"Directory \"{dirName}\" created");
                    }
                    return true;
                }
            }
            catch (System.Exception)
            {
                throw new FSOpsException($"Failed to create directory \"{dirName}\"");
            }
        }

        public static void removeResourceFilesIfPresent(ResourceType resourceType, string resourceName)
        {
            try
            {
                string resourceFilePath = Path.Join(resourceType.ToString().ToLower(), resourceName);
                string resourceVersionFilePath = Path.Join(resourceType.ToString().ToLower(), nyokaFolderName, resourceName + nyokaVersionExtension);

                if (File.Exists(resourceFilePath))
                {
                    File.Delete(resourceFilePath);
                }

                if (File.Exists(resourceVersionFilePath))
                {
                    File.Delete(resourceVersionFilePath);
                }
            }
            catch (System.Exception)
            {
                throw new FSOpsException($"Failed to remove {resourceType.ToString().ToLower()} resource {resourceName}");
            }
        }
        
        public static bool resourceFileExists(ResourceType resourceType, string resourceName)
        {
            try
            {
                return File.Exists(Path.Join(resourceType.ToString().ToLower(), resourceName));
            }
            catch (System.Exception)
            {
                throw new FSOpsException($"Failed to check file system for whether {resourceType.ToString().ToLower()} resource {resourceName} exists");
            }
        }

        public static bool resourceVersionFileExists(ResourceType resourceType, string resourceName)
        {
            try
            {
                return File.Exists(Path.Join(resourceType.ToString().ToLower(), nyokaFolderName, resourceName + nyokaVersionExtension));
            }
            catch (System.Exception)
            {
                throw new FSOpsException($"Failed to check for existence of version file for {resourceType.ToString().ToLower().ToLower()} resource {resourceName}");
            }
        }

        public static IEnumerable<string> resourceNames(ResourceType resourceType)
        {
            try
            {
                return new DirectoryInfo(resourceType.ToString().ToLower().ToLower()).EnumerateFiles().Select(file => file.Name);
            }
            catch (System.Exception)
            {
                throw new FSOpsException($"Failed to get list of {resourceType.ToString().ToLower().ToLower()} resources from file system");
            }
        }

        public static FileStream createResourceFile(ResourceType resourceType, string resourceName)
        {
            try
            {
                return File.Create(Path.Join(resourceType.ToString().ToLower(), resourceName));
            }
            catch (System.Exception)
            {
                throw new FSOpsException($"Failed to create file for {resourceType.ToString().ToLower()} resource {resourceName}");
            }
        }

        public static StreamWriter createOrOverwriteResourceVersionFile(ResourceType resourceType, string resourceName)
        {
            try
            {
                string filePath = Path.Join(resourceType.ToString().ToLower(), nyokaFolderName, resourceName + nyokaVersionExtension);
                if  (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                
                return File.CreateText(filePath);
            }
            catch (System.Exception)
            {
                throw new FSOpsException($"Failed to create metadata file for {resourceType.ToString().ToLower()} resource {resourceName}");
            }
        }

        public static string getResourceVersion(ResourceType resourceType, string resourceName)
        {
            try
            {
                return File.ReadAllText(Path.Join(resourceType.ToString().ToLower(), nyokaFolderName, resourceName + nyokaVersionExtension)).Trim();
            }
            catch (System.Exception)
            {
                throw new FSOpsException($"Failed to read metadata file for {resourceType.ToString().ToLower()} resource {resourceName}");
            }
        }

        public static long getResourceSize(ResourceType resourceType, string resourceName)
        {
            try
            {
                return new FileInfo(Path.Join(resourceType.ToString().ToLower(), resourceName)).Length;
            }
            catch (System.Exception)
            {
                throw new FSOpsException($"Failed to determine file size of file for {resourceType.ToString().ToLower()} resource {resourceName}");
            }
        }
        // public static bool checkPublishFileExists(string resourceName)
        // {
        //     try
        //     {
        //         return File.Exists(resourceName);
        //     }
        //     catch (System.Exception)
        //     {
        //         throw new FSOpsException($"Failed to check for existence of file {resourceName}");
        //     }
        // }

        public static FileStream readResourceFile(ResourceType resourceType, string fileName)
        {
            try
            {
                string filePath = Path.Join(resourceType.ToString().ToLower(), fileName);
                return File.OpenRead(filePath);
            }
            catch (System.Exception)
            {
                throw new FSOpsException($"Failed to open {resourceType.ToString().ToLower()} resource {fileName}. Does this file exist?");
            }
        }
    }
}