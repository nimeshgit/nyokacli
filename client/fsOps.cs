using System.Collections.Generic;
using System.Linq;
using System.IO;
using Constants;
using CLIInterfaceNS;
using NyokaRemoteNS;
using Newtonsoft.Json;

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

        public static char dirSeparatorChar => Path.DirectorySeparatorChar;

        private static readonly string remoteServerConfigFileName = "nyokaremote.json";
        private static readonly string codeDirName = "Code";
        private static readonly string dataDirName = "Data";
        private static readonly string modelDirName = "Models";
        private static readonly string nyokaFolderName = ".nyoka";

        private static readonly string nyokaVersionExtension = ".version";
        private static readonly string[] dirNames = new string[] { codeDirName, dataDirName, modelDirName };

        private static string resourceDirPath(ResourceType resourceType)
        {
            if (resourceType == ResourceType.Code) return codeDirName;
            if (resourceType == ResourceType.Data) return dataDirName;
            if (resourceType == ResourceType.Model) return modelDirName;
            throw new FSOpsException($"No matching resource directory for resource type {resourceType}");
        }

        public static bool remoteServerConfigFileExists()
        {
            return File.Exists(remoteServerConfigFileName);
        }

        public static string unsafeGetRemoteServerConfigString(string prefix)
        {
            NyokaRemote nyremote = JsonConvert.DeserializeObject<NyokaRemote>(File.ReadAllText(remoteServerConfigFileName));
            if (prefix=="-s" || prefix=="--zementisserver")
            {
                return nyremote.ZementisServer;
            }
            else if (prefix=="-m" || prefix=="--zementismodeler")
            {
                return nyremote.ZementisModeler;
            }
            else
            {
                return nyremote.RepositoryServer;
            }
        }

        public static void createOrOverwriteRemoteServerConfigString(string prefix , string serverAddress)
        {
            NyokaRemote nyokaRemote;
            // First Scenario
            if (!remoteServerConfigFileExists())
            {
                nyokaRemote = new NyokaRemote
                {
                RepositoryServer = null,
                ZementisServer = null,
                ZementisModeler = null                
                };                
            }
            else
            {
                nyokaRemote = JsonConvert.DeserializeObject<NyokaRemote>(File.ReadAllText(remoteServerConfigFileName));
            }
            if (prefix == "-s" || prefix == "--zementisserver")
            {
                nyokaRemote.ZementisServer = serverAddress; 
            }
            else if(prefix == "-m" || prefix =="--zementismodeler")
            {
                nyokaRemote.ZementisModeler = serverAddress;
            }
            else 
            {
                nyokaRemote.RepositoryServer = serverAddress;
            }         
            string nyremo = JsonConvert.SerializeObject(nyokaRemote,Formatting.Indented);
            File.WriteAllText(remoteServerConfigFileName,nyremo);
        }
        
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
                string resourceFilePath = Path.Join(resourceDirPath(resourceType), resourceName);
                string resourceVersionFilePath = Path.Join(resourceDirPath(resourceType), nyokaFolderName, resourceName + nyokaVersionExtension);

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
                return File.Exists(Path.Join(resourceDirPath(resourceType), resourceName));
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
                return File.Exists(Path.Join(resourceDirPath(resourceType), nyokaFolderName, resourceName + nyokaVersionExtension));
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
                return new DirectoryInfo(resourceDirPath(resourceType)).EnumerateFiles().Select(file => file.Name);
            }
            catch (System.Exception)
            {
                throw new FSOpsException($"Failed to get list of {resourceType.ToString()} resources from file system");
            }
        }

        public static FileStream createResourceFile(ResourceType resourceType, string resourceName)
        {
            try
            {
                return File.Create(Path.Join(resourceDirPath(resourceType), resourceName));
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
                string filePath = Path.Join(resourceDirPath(resourceType), nyokaFolderName, resourceName + nyokaVersionExtension);
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
                return File.ReadAllText(Path.Join(resourceDirPath(resourceType), nyokaFolderName, resourceName + nyokaVersionExtension)).Trim();
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
                return new FileInfo(Path.Join(resourceDirPath(resourceType), resourceName)).Length;
            }
            catch (System.Exception)
            {
                throw new FSOpsException($"Failed to determine file size of file for {resourceType.ToString().ToLower()} resource {resourceName}");
            }
        }

        public static FileStream readResourceFile(ResourceType resourceType, string fileName)
        {
            try
            {
                string filePath = Path.Join(resourceDirPath(resourceType), fileName);
                return File.OpenRead(filePath);
            }
            catch (System.Exception)
            {
                throw new FSOpsException($"Failed to open {resourceType.ToString().ToLower()} resource {fileName}. Does this file exist?");
            }
        }
    }
   
}