using System.Collections.Generic;

namespace FSOpsNS {
    public static class FSOps {
        public static List<string> a() {
            return new List<string>();
        }
        public static bool createCodeDataModelDirs(bool logExisting = false, bool logCreated = false, bool logError = true) {
            bool successful = true;
            
            foreach (string dirName in new string[] { "code", "data", "model" }) {
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
    }
}