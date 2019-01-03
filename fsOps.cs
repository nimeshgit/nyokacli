using System.Collections.Generic;
using System.Linq;

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
                if (!System.IO.Directory.Exists(dirNames)) {
                    return false;
                }
            }

            return true;
        }
        
        public static bool createCodeDataModelDirs(bool logExisting = false, bool logCreated = false, bool logError = true) {
            bool successful = true;
            
            foreach (string dirName in FSOps.dirNames) {
                if (System.IO.Directory.Exists(dirName)) {
                    if (logExisting) {
                        System.Console.WriteLine($"Directory \"{dirName}\" already exists");
                    }
                } else {
                    try {
                        System.IO.Directory.CreateDirectory(dirName);
                        
                        if (logCreated) {
                            System.Console.WriteLine($"Directory \"{dirName}\" created");
                        }
                    } catch (System.IO.IOException) {
                        if (logError) {
                            System.Console.WriteLine($"Failed to create directory \"{dirName}\"");
                        }
                        successful = false;
                    }
                }
            }

            return successful;
        }

        public static IEnumerable<string> codeResourceNames() {
            return new System.IO.DirectoryInfo(FSOps.codeDirName).EnumerateFiles().Select(file => file.Name);
        }

        public static IEnumerable<string> dataResourceNames() {
            return new System.IO.DirectoryInfo(FSOps.dataDirName).EnumerateFiles().Select(file => file.Name);
        }

        public static IEnumerable<string> modelResourceNames() {
            return new System.IO.DirectoryInfo(FSOps.modelDirName).EnumerateFiles().Select(file => file.Name);
        }
    }
}