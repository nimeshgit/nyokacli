using InfoTransferContainers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ServerResourceDirNS
{
    public class ServerResourceDir
    {
        private static string getMimeType(FileInfo fileInfo) {
            string extension = fileInfo.Name.Substring(fileInfo.Name.LastIndexOf(".") + 1);

            switch (extension) {
            case "py":    return "application/x-python-code";
            case "ipynb": return "application/x-ipynb+json";
            case "csv":   return "text/csv";
            case "json":  return "application/json";
            case "png":   return "image/png";
            case "jpg":
            case "jpeg":  return "image/jpeg";
            case "pmml":  return "application/xml";
            default: throw new System.ArgumentException();
            }
        }

        private static FileInfoTransferContainer createServerFileInfo(FileInfo fileInfo) {
            return new FileInfoTransferContainer(
                fileInfo.Length,
                "???"
            );
        }

        private static int compareVersions(string v1, string v2) {
            List<int> v1Sections = v1.Split(".").Select(sec => int.Parse(sec)).ToList();
            List<int> v2Sections = v2.Split(".").Select(sec => int.Parse(sec)).ToList();

            for (int pos = 0;; pos++) {
                if (pos == v1Sections.Count && pos == v2Sections.Count) return 0;
                if (pos == v1Sections.Count) return 1;
                if (pos == v2Sections.Count) return -1;

                if (v1Sections[pos] > v2Sections[pos]) return 1;
                if (v1Sections[pos] < v2Sections[pos]) return -1;
            }
        }

        private string root;
        private string codeDirPath => $"{root}/Code";
        private string dataDirPath => $"{root}/Data";
        private string modelDirPath => $"{root}/Model";

        private Dictionary<string, FileInfoTransferContainer> getDirServerInfoDict(string parentDirPath) {
            Dictionary<string, FileInfoTransferContainer> infoDict = new Dictionary<string, FileInfoTransferContainer>();
            
            List<string> dirNames = Directory.GetDirectories(parentDirPath)
                .Select(dirPath => new DirectoryInfo(dirPath).Name).ToList();
            
            foreach (string dirName in dirNames) {                
                string resourceDirPath = Path.Combine(parentDirPath, dirName);
                List<string> versions = Directory.GetDirectories(resourceDirPath)
                    .Select(path => new DirectoryInfo(path).Name).ToList();

                versions.Sort(compareVersions);
                
                string latestVersion = versions[versions.Count - 1];
                
                string resourceFilePath = Path.Combine(
                    parentDirPath,
                    dirName,
                    latestVersion,
                    dirName
                );

                infoDict[dirName] = new FileInfoTransferContainer(
                    new FileInfo(resourceFilePath).Length,
                    latestVersion
                );
            }

            return infoDict;
        }

        private FileStreamResult getDirFileStreamResult(string codeDirPath, string resourceName, string version) {
            string filePath = Path.Combine(codeDirPath, resourceName, version, resourceName);
            System.Console.WriteLine(filePath);
            return new FileStreamResult(
                File.OpenRead(filePath),
                getMimeType(new FileInfo(filePath))
            );
        }

        private class DepsFileJson {
            public class DepsFileEntry {
                public string version;
            }
            
            public Dictionary<string, DepsFileEntry> code;
            public Dictionary<string, DepsFileEntry> data;
            public Dictionary<string, DepsFileEntry> model;

            public DepsFileJson(
                Dictionary<string, DepsFileEntry> code,
                Dictionary<string, DepsFileEntry> data,
                Dictionary<string, DepsFileEntry> model
            ) {
                this.code = code;
                this.data = data;
                this.model = model;
            }
        }

        private ResourceInfoContainer getDirResourceDeps(string codeDirPath, string version, string resourceName)
        {
            string depsFilePath = Path.Combine(
                codeDirPath,
                resourceName,
                version,
                resourceName + ".deps"
            );
            
            string depsJson = File.ReadAllText(depsFilePath);
            DepsFileJson fileJson = JsonConvert.DeserializeObject<DepsFileJson>(depsJson);

            return new ResourceInfoContainer(
                fileJson.code.ToDictionary(p => p.Key, p => new ResourceInfoContainer.DependencyDescription(p.Value.version)),
                fileJson.data.ToDictionary(p => p.Key, p => new ResourceInfoContainer.DependencyDescription(p.Value.version)),
                fileJson.model.ToDictionary(p => p.Key, p => new ResourceInfoContainer.DependencyDescription(p.Value.version))
            );
        }
        
        public ServerResourceDir(string pathArg)
        {
            root = pathArg;
        }

        public FileStreamResult getCodeStream(string fileName, string version) => getDirFileStreamResult(codeDirPath, fileName, version);
        public FileStreamResult getDataStream(string fileName, string version) => getDirFileStreamResult(dataDirPath, fileName, version);
        public FileStreamResult getModelStream(string fileName, string version) => getDirFileStreamResult(modelDirPath, fileName, version);

        public Dictionary<string, FileInfoTransferContainer> getCodeServerInfoDict() => getDirServerInfoDict(codeDirPath);
        public Dictionary<string, FileInfoTransferContainer> getDataServerInfoDict() => getDirServerInfoDict(dataDirPath);
        public Dictionary<string, FileInfoTransferContainer> getModelServerInfoDict() => getDirServerInfoDict(modelDirPath);

        public ResourceInfoContainer getCodeResourceDeps(string resourceName, string version) => getDirResourceDeps(codeDirPath, version, resourceName);
        public ResourceInfoContainer getDataResourceDeps(string resourceName, string version) => getDirResourceDeps(dataDirPath, version, resourceName);
        public ResourceInfoContainer getModelResourceDeps(string resourceName, string version) => getDirResourceDeps(modelDirPath, version, resourceName);
    }
}