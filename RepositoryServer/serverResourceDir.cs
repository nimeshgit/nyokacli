using InfoTransferContainers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace ServerResourceDirNS
{
    public class ServerResourceDir
    {
        private static string depsFileExtension = ".deps";
        private static string getMimeType(FileInfo fileInfo)
        {
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


        private static int compareVersions(string v1, string v2)
        {
            List<int> v1Sections = v1.Split(".").Select(sec => int.Parse(sec)).ToList();
            List<int> v2Sections = v2.Split(".").Select(sec => int.Parse(sec)).ToList();

            for (int pos = 0;; pos++)
            {
                if (pos == v1Sections.Count && pos == v2Sections.Count) return 0;
                if (pos == v1Sections.Count) return -1;
                if (pos == v2Sections.Count) return 1;

                if (v1Sections[pos] > v2Sections[pos]) return -1;
                if (v1Sections[pos] < v2Sections[pos]) return 1;
            }
        }

        private string root;
        private string codeDirPath => $"{root}/Code";
        private string dataDirPath => $"{root}/Data";
        private string modelDirPath => $"{root}/Model";

        private Dictionary<string, FileInfoTransferContainer> getDirServerInfoDict(string parentDirPath)
        {
            Dictionary<string, FileInfoTransferContainer> infoDict = new Dictionary<string, FileInfoTransferContainer>();

            List<string> resourceNames = Directory.GetDirectories(parentDirPath)
                .Select(dirPath => new DirectoryInfo(dirPath).Name).ToList();

            foreach (string resourceName in resourceNames) {
                string resourceDirPath = Path.Combine(parentDirPath, resourceName);

                List<string> versions = Directory.GetDirectories(resourceDirPath)
                    .Select(path => new DirectoryInfo(path).Name).ToList();

                versions.Sort(compareVersions);

                // first item in sorted list is latest version
                string latestVersion = versions[0];

                string resourceFilePath = Path.Combine(
                    parentDirPath,
                    resourceName,
                    latestVersion,
                    resourceName
                );

                infoDict[resourceName] = new FileInfoTransferContainer(
                    resourceFileSize(parentDirPath, latestVersion, resourceName),
                    latestVersion
                );
            }

            return infoDict;
        }

        private FileStreamResult getDirFileStreamResult(string parentDirPath, string resourceName, string version)
        {
            string filePath = Path.Combine(parentDirPath, resourceName, version, resourceName);

            return new FileStreamResult(
                File.OpenRead(filePath),
                getMimeType(new FileInfo(filePath))
            );
        }

        private class DepsFileJson
        {
            public class DepsFileEntry
            {
                public string key;
                public string version;

                public DepsFileEntry(string key, string version)
                {
                    this.key = key;
                    this.version = version;
                }
            }

            public List<DepsFileEntry> code;
            public List<DepsFileEntry> data;
            public List<DepsFileEntry> model;

            public DepsFileJson(
                List<DepsFileEntry> code,
                List<DepsFileEntry> data,
                List<DepsFileEntry> model
            ) {
                this.code = code;
                this.data = data;
                this.model = model;
            }
        }

        private long resourceFileSize(string parentDirPath, string version, string resourceName)
        {
            return new FileInfo(Path.Combine(parentDirPath, resourceName, version, resourceName)).Length;
        }

        private ResourceDependencyInfoContainer getDirResourceDeps(string parentDirPath, string version, string resourceName)
        {
            string depsFilePath = Path.Combine(
                parentDirPath,
                resourceName,
                version,
                resourceName + depsFileExtension
            );


            string depsJson = File.ReadAllText(depsFilePath);
            DepsFileJson fileJson = JsonConvert.DeserializeObject<DepsFileJson>(depsJson);

            ResourceDependencyInfoContainer infoContainer = new ResourceDependencyInfoContainer();

            // cloning
            List<DepsFileJson.DepsFileEntry> codeUninvestigated = fileJson.code.Select(x => x).ToList();
            List<DepsFileJson.DepsFileEntry> dataUninvestigated = fileJson.data.Select(x => x).ToList();
            List<DepsFileJson.DepsFileEntry> modelUninvestigated = fileJson.model.Select(x => x).ToList();

            while (true)
            {
                string dependenciesOfDependencyPath = null;

                (List<DepsFileJson.DepsFileEntry>, Dictionary<string, ResourceDependencyInfoContainer.DependencyDescription>, List<DepsFileJson.DepsFileEntry>, string)[] resourceGroups = {
                    (codeUninvestigated, infoContainer.codeDeps, fileJson.code, codeDirPath),
                    (dataUninvestigated, infoContainer.dataDeps, fileJson.data, dataDirPath),
                    (modelUninvestigated, infoContainer.modelDeps, fileJson.model, modelDirPath),
                };

                bool noMoreUninvestigated = true;
                foreach (var (resourceUninvestigated, infoContainerResourceDeps, directResourceDeps, resourceDirPath) in resourceGroups)
                {
                    if (resourceUninvestigated.Count != 0)
                    {
                        noMoreUninvestigated = false;
                        DepsFileJson.DepsFileEntry investigating = resourceUninvestigated.First();

                        infoContainerResourceDeps[investigating.key] = new ResourceDependencyInfoContainer.DependencyDescription(
                            investigating.version,
                            directResourceDeps.Any(v => v.key == investigating.key),
                            resourceFileSize(resourceDirPath, investigating.version, investigating.key)
                        );

                        dependenciesOfDependencyPath = Path.Combine(
                            resourceDirPath,
                            investigating.key,
                            investigating.version,
                            investigating.key + depsFileExtension
                        );

                        resourceUninvestigated.RemoveAt(0);
                        break;
                    }
                }

                if (noMoreUninvestigated)
                {
                    break;
                }
                else
                {
                    DepsFileJson dependenciesOfDependency = JsonConvert.DeserializeObject<DepsFileJson>(File.ReadAllText(dependenciesOfDependencyPath));

                    (List<DepsFileJson.DepsFileEntry>, List<DepsFileJson.DepsFileEntry>)[] assignPairs = {
                        (dependenciesOfDependency.code, codeUninvestigated),
                        (dependenciesOfDependency.data, dataUninvestigated),
                        (dependenciesOfDependency.model, modelUninvestigated)
                    };

                    foreach (var (newDeps, mergeIntoDeps) in assignPairs)
                    {
                        foreach (var newDepInfo in newDeps)
                        {
                            if (!mergeIntoDeps.Any(mergeInfo => mergeInfo.key == newDepInfo.key))
                            {
                                mergeIntoDeps.Add(newDepInfo);
                            }
                        }
                    }
                }
            }

            return infoContainer;
        }

        private ResourceVersionsInfoContainer getResourceVersions(string parentDirPath, string resourceName)
        {
            List<string> versions = Directory.GetDirectories(Path.Combine(parentDirPath, resourceName))
                .Select(path => new DirectoryInfo(path).Name).ToList();

            versions.Sort(compareVersions);

            return new ResourceVersionsInfoContainer(
                versions,
                versions[0] // first item in sorted list is the newest version
            );
        }

        private List<DepsFileJson.DepsFileEntry> publishDepsInfosToDepsFileEntries(Dictionary<string, PublishDepsInfoContainer.PublishDepDescription> depDescriptionDict)
        {
            var depsEntries = new List<DepsFileJson.DepsFileEntry>();

            foreach (var (depName, depDescription) in depDescriptionDict.Select(x => (x.Key, x.Value)))
            {
                depsEntries.Add(new DepsFileJson.DepsFileEntry(depName, depDescription.version));
            }

            return depsEntries;
        }

        private void addResource(string parentDirPath, string resourceName, string version, PublishDepsInfoContainer depsInfoContainer, Stream inputResourceStream)
        {
            string resourceDir = Path.Combine(parentDirPath, resourceName);
            if (!Directory.Exists(resourceDir))
            {
                Directory.CreateDirectory(resourceDir);
            }

            string resourceVersionDir = Path.Combine(resourceDir, version);
            if (!Directory.Exists(resourceVersionDir))
            {
                Directory.CreateDirectory(resourceVersionDir);
            }

            string resourceFilePath = Path.Combine(
                resourceVersionDir,
                resourceName
            );
            string resourceDepsPath = Path.Combine(
                resourceVersionDir,
                resourceName + depsFileExtension
            );

            if (File.Exists(resourceFilePath)) File.Delete(resourceFilePath);
            if (File.Exists(resourceDepsPath)) File.Delete(resourceFilePath);

            using (Stream resourceFileStream = File.OpenWrite(resourceFilePath))
            using (var resourceDepsStream = new StreamWriter(File.OpenWrite(resourceDepsPath)))
            {
                inputResourceStream.CopyTo(resourceFileStream);

                var depsToSerialize = new DepsFileJson(
                    publishDepsInfosToDepsFileEntries(depsInfoContainer.codeDeps),
                    publishDepsInfosToDepsFileEntries(depsInfoContainer.dataDeps),
                    publishDepsInfosToDepsFileEntries(depsInfoContainer.modelDeps)
                );

                string serializedDeps = JsonConvert.SerializeObject(depsToSerialize);

                resourceDepsStream.Write(serializedDeps);
            }
        }

        public ServerResourceDir(string pathArg)
        {
            root = pathArg;
        }

        public ResourceVersionsInfoContainer getCodeVersions(string resourceName) => getResourceVersions(codeDirPath, resourceName);
        public ResourceVersionsInfoContainer getDataVersions(string resourceName) => getResourceVersions(dataDirPath, resourceName);
        public ResourceVersionsInfoContainer getModelVersions(string resourceName) => getResourceVersions(modelDirPath, resourceName);

        public FileStreamResult getCodeStream(string fileName, string version) => getDirFileStreamResult(codeDirPath, fileName, version);
        public FileStreamResult getDataStream(string fileName, string version) => getDirFileStreamResult(dataDirPath, fileName, version);
        public FileStreamResult getModelStream(string fileName, string version) => getDirFileStreamResult(modelDirPath, fileName, version);

        public Dictionary<string, FileInfoTransferContainer> getCodeServerInfoDict() => getDirServerInfoDict(codeDirPath);
        public Dictionary<string, FileInfoTransferContainer> getDataServerInfoDict() => getDirServerInfoDict(dataDirPath);
        public Dictionary<string, FileInfoTransferContainer> getModelServerInfoDict() => getDirServerInfoDict(modelDirPath);

        public ResourceDependencyInfoContainer getCodeResourceDeps(string resourceName, string version) => getDirResourceDeps(codeDirPath, version, resourceName);
        public ResourceDependencyInfoContainer getDataResourceDeps(string resourceName, string version) => getDirResourceDeps(dataDirPath, version, resourceName);
        public ResourceDependencyInfoContainer getModelResourceDeps(string resourceName, string version) => getDirResourceDeps(modelDirPath, version, resourceName);

        public void addCodeResource(string resourceName, string version, PublishDepsInfoContainer depsInfoContainer, Stream resourceFileStream) =>
            addResource(codeDirPath, resourceName, version, depsInfoContainer, resourceFileStream);
        public void addDataResource(string resourceName, string version, PublishDepsInfoContainer depsInfoContainer, Stream resourceFileStream) =>
            addResource(dataDirPath, resourceName, version, depsInfoContainer, resourceFileStream);
        public void addModelResource(string resourceName, string version, PublishDepsInfoContainer depsInfoContainer, Stream resourceFileStream) =>
            addResource(modelDirPath, resourceName, version, depsInfoContainer, resourceFileStream);
    }
}
