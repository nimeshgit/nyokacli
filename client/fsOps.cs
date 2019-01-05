using System.Collections.Generic;
using System.Linq;
using System.IO;
using Constants;
using NyokaInfoContainerNS;
using Newtonsoft.Json;

namespace FSOpsNS
{
    public static class FSOps
    {
        private static readonly string codeDirName = "code";
        private static readonly string dataDirName = "data";
        private static readonly string modelDirName = "model";
        private static readonly string nyokaFileName = ".nyoka";
        private static readonly string[] dirNames = new string[] { codeDirName, dataDirName, modelDirName };
        
        public static bool hasNecessaryDirsAndFiles()
        {
            foreach (string dirNames in dirNames)
            {
                if (!Directory.Exists(dirNames))
                {
                    return false;
                }
            }

            if (!File.Exists(nyokaFileName))
            {
                return false;
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
                if (Directory.Exists(dirName))
                {
                    if (logExisting)
                    {
                        System.Console.WriteLine($"Directory \"{dirName}\" already exists");
                    }
                } 
                else
                {
                    try
                    {
                        Directory.CreateDirectory(dirName);
                        
                        if (logCreated)
                        {
                            System.Console.WriteLine($"Directory \"{dirName}\" created");
                        }
                    }
                    catch (IOException)
                    {
                        if (logError)
                        {
                            System.Console.WriteLine($"Failed to create directory \"{dirName}\"");
                        }
                        successful = false;
                    }
                }
            }

            if (File.Exists(nyokaFileName))
            {
                if (logExisting)
                {
                    System.Console.WriteLine($"File \"{nyokaFileName}\" already exists");
                }
            }
            else
            {
                try
                {
                    using (StreamWriter writer = File.CreateText(nyokaFileName))
                    {
                        NyokaInfoContainer emptyContainer = new NyokaInfoContainer();
                        writer.Write(JsonConvert.SerializeObject(emptyContainer));
                    }
                    
                    if (logCreated)
                    {
                        System.Console.WriteLine($"File \"{nyokaFileName}\" created");
                    }
                }
                catch (IOException)
                {
                    if (logError)
                    {
                        System.Console.WriteLine($"Failed to create file \"{nyokaFileName}\"");
                    }
                    successful = false;
                }
            }

            return successful;
        }

        public static void removeResource(ResourceType resourceType, string resourceName)
        {
            File.Delete($"{resourceType.ToString()}/{resourceName}");
        }
        
        public static bool resourceExists(ResourceType resourceType, string resourceName)
        {
            return File.Exists($"{resourceType.ToString()}/{resourceName}");
        }

        public static IEnumerable<string> resourceNames(ResourceType resourceType)
        {
            return new DirectoryInfo(resourceType.ToString()).EnumerateFiles().Select(file => file.Name);
        }

        public static FileStream createResourceFile(ResourceType resourceType, string resourceName)
        {
            return File.Create($"{resourceType.ToString()}/{resourceName}");
        }
    }
}