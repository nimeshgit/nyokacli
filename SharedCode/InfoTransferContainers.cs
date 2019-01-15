using System;
using System.Collections.Generic;
using System.Linq;

namespace InfoTransferContainers
{
    internal static class Splitter
    {
        public static List<string> trimThenHandlEmptyString(string str, char splitCh)
        {
            if (str.Trim().Length == 0) return new List<string>();
            
            // trim then split
            return str.Trim().Split(splitCh).ToList();
        }
    }
    public class AvailableResourcesInfoContainer
    {
        
        public class AvailableResourceDescription
        {
            public static AvailableResourceDescription deserialize(string str)
            {
                List<string> splitBySemicolon = Splitter.trimThenHandlEmptyString(str, ';');
                return new AvailableResourceDescription(
                    long.Parse(splitBySemicolon[0]),
                    splitBySemicolon[1]
                );
            }
            
            public long byteCount;
            public string versionStr;

            public AvailableResourceDescription(long byteCount, string versionStr)
            {
                this.byteCount = byteCount;
                this.versionStr = versionStr;
            }

            public string serialize()
            {
                return $"{byteCount.ToString()};{versionStr}";
            }
        }
        public static AvailableResourcesInfoContainer deserialize(string str)
        {
            List<string> keyValueStrings = Splitter.trimThenHandlEmptyString(str, ',');

            var resourceDescriptions = new Dictionary<string, AvailableResourceDescription>();
            
            foreach (string keyValueStr in keyValueStrings)
            {
                string key = keyValueStr.Split(':')[0];
                string resourceDescriptionStr = keyValueStr.Split(':')[1];

                resourceDescriptions[key] = AvailableResourceDescription.deserialize(resourceDescriptionStr);
            }
            
            return new AvailableResourcesInfoContainer(resourceDescriptions);
        }

        public Dictionary<string, AvailableResourceDescription> resourceDescriptions;
        public AvailableResourcesInfoContainer(Dictionary<string, AvailableResourceDescription> resourceDescriptions)
        {
            this.resourceDescriptions = resourceDescriptions;
        }

        public AvailableResourcesInfoContainer()
        {
            this.resourceDescriptions = new Dictionary<string, AvailableResourceDescription>();
        }

        public string serialize()
        {
            List<string> keyValueStrings = new List<string>();

            foreach (var (key, resourceDescription) in resourceDescriptions.Select(x => (x.Key, x.Value)))
            {
                keyValueStrings.Add($"{key}:{resourceDescription.serialize()}");
            }

            return string.Join(",", keyValueStrings);
        }
    }

    public class ResourceVersionsInfoContainer
    {
        public static ResourceVersionsInfoContainer deserialize(string str)
        {
            string[] splitBySemicolon = str.Trim().Split(';');
            string latestVersion = splitBySemicolon[0];
            List<string> versions = Splitter.trimThenHandlEmptyString(splitBySemicolon[1], ',');

            return new ResourceVersionsInfoContainer(versions, latestVersion);
        }
        
        public List<string> versions;
        public string latestVersion;

        public ResourceVersionsInfoContainer(List<string> versions, string latestVersion)
        {
            this.versions = versions;
            this.latestVersion = latestVersion;
        }

        public string serialize()
        {
            return $"{latestVersion};{string.Join(",", versions)}";
        }
    }

    public class ResourceDependencyInfoContainer
    {
        public class DependencyDescription
        {
            public static DependencyDescription deserialize(string str)
            {
                string[] split = str.Trim().Split('-');
                string versionStr = split[0];
                bool isDirectDependency = bool.Parse(split[1]);
                long byteCount = long.Parse(split[2]);

                return new DependencyDescription(versionStr, isDirectDependency, byteCount);
            }
            public bool isDirectDependency;
            public string versionStr;
            public long byteCount;

            public DependencyDescription(string versionStr, bool isDirectDependency, long byteCount)
            {
                this.versionStr = versionStr;
                this.isDirectDependency = isDirectDependency;
                this.byteCount = byteCount;
            }

            public string serialize()
            {
                return $"{versionStr}-{isDirectDependency.ToString()}-{byteCount.ToString()}";
            }
        }

        public static ResourceDependencyInfoContainer deserialize(string str)
        {
            List<string> depDictsStrings = Splitter.trimThenHandlEmptyString(str, ';');

            var depDicts = new List<Dictionary<string, DependencyDescription>>();

            foreach (string depDictStr in depDictsStrings)
            {
                var depDict = new Dictionary<string, DependencyDescription>();

                foreach (string keyValStrPair in Splitter.trimThenHandlEmptyString(depDictStr, ','))
                {
                    string key = keyValStrPair.Split(':')[0];
                    string depDescriptionStr = keyValStrPair.Split(':')[1];

                    depDict[key] = DependencyDescription.deserialize(depDescriptionStr);
                }

                depDicts.Add(depDict);
            }

            var codeDeps = depDicts[0];
            var dataDeps = depDicts[1];
            var modelDeps = depDicts[2];

            return new ResourceDependencyInfoContainer(codeDeps, dataDeps, modelDeps);
        }

        public Dictionary<string, DependencyDescription> codeDeps;
        public Dictionary<string, DependencyDescription> dataDeps;
        public Dictionary<string, DependencyDescription> modelDeps;

        public ResourceDependencyInfoContainer(
            Dictionary<string, DependencyDescription> codeDeps,
            Dictionary<string, DependencyDescription> dataDeps,
            Dictionary<string, DependencyDescription> modelDeps)
        {
            this.codeDeps = codeDeps;
            this.dataDeps = dataDeps;
            this.modelDeps = modelDeps;
        }

        public ResourceDependencyInfoContainer()
        {
            this.codeDeps = new Dictionary<string, DependencyDescription>();
            this.dataDeps = new Dictionary<string, DependencyDescription>();
            this.modelDeps = new Dictionary<string, DependencyDescription>();
        }

        public string serialize()
        {
            Dictionary<string, DependencyDescription>[] depDicts = {codeDeps, dataDeps, modelDeps};

            List<string> serializedDicts = new List<string>();
            
            foreach (var depDict in depDicts)
            {
                List<string> dictEntries = new List<string>();
                foreach (var (depName, depDescription) in depDict.Select(x => (x.Key, x.Value)))
                {
                    dictEntries.Add($"{depName}:{depDescription.serialize()}");
                }
                string serializedDict = string.Join(",", dictEntries);
                serializedDicts.Add(serializedDict);
            }

            string serializedInfoContainer = string.Join(";", serializedDicts);

            return serializedInfoContainer;
        }
    }

    public class PublishDepsInfoContainer
    {
        public class PublishDepDescription
        {
            public static PublishDepDescription deserialize(string str)
            {
                return new PublishDepDescription(str.Trim());
            }
            public string version;

            public PublishDepDescription(string version)
            {
                this.version = version;
            }

            public string serialize()
            {
                return $"{version}";
            }
        }

        public Dictionary<string, PublishDepDescription> codeDeps;
        public Dictionary<string, PublishDepDescription> dataDeps;
        public Dictionary<string, PublishDepDescription> modelDeps;

        public static PublishDepsInfoContainer deserialize(string str)
        {
            List<string> depDictStrings = Splitter.trimThenHandlEmptyString(str, ';');

            var depDicts = new List<Dictionary<string, PublishDepDescription>>();

            foreach (string depDictString in depDictStrings)
            {
                var depDict = new Dictionary<string, PublishDepDescription>();
                List<string> depKeyValuePairStrings = Splitter.trimThenHandlEmptyString(depDictString, ',');

                foreach (string depKVPairStr in depKeyValuePairStrings)
                {
                    string key = depKVPairStr.Split(':')[0];
                    string depDescriptionStr = depKVPairStr.Split(':')[1];

                    depDict[key] = PublishDepDescription.deserialize(depDescriptionStr);
                }

                depDicts.Add(depDict);
            }

            var codeDeps = depDicts[0];
            var dataDeps = depDicts[1];
            var modelDeps = depDicts[2];

            return new PublishDepsInfoContainer(codeDeps, dataDeps, modelDeps);
        }

        public PublishDepsInfoContainer()
        {
            this.codeDeps = new Dictionary<string, PublishDepDescription>();
            this.dataDeps = new Dictionary<string, PublishDepDescription>();
            this.modelDeps = new Dictionary<string, PublishDepDescription>();
        }

        public PublishDepsInfoContainer(
            Dictionary<string, PublishDepDescription> codeDeps,
            Dictionary<string, PublishDepDescription> dataDeps,
            Dictionary<string, PublishDepDescription> modelDeps)
        {
            this.codeDeps = codeDeps;
            this.dataDeps = dataDeps;
            this.modelDeps = modelDeps;
        }

        public string serialize()
        {
            Dictionary<string, PublishDepDescription>[] depDicts = {codeDeps, dataDeps, modelDeps};
            List<string> serializedDepDicts = new List<string>();

            foreach (Dictionary<string, PublishDepDescription> depDict in depDicts)
            {
                List<string> depDictKeyValueStrings = new List<string>();

                foreach (var (depName, depDescription) in depDict.Select(x => (x.Key, x.Value)))
                {
                    string keyValuePairStr = $"{depName}:{depDescription.serialize()}";
                    depDictKeyValueStrings.Add(keyValuePairStr);
                }

                string dictSerializedStr = string.Join(",", depDictKeyValueStrings);
                serializedDepDicts.Add(dictSerializedStr);
            }

            string depInfoContainerInfoString = string.Join(";", serializedDepDicts);
            return depInfoContainerInfoString;
        }
    }
}
