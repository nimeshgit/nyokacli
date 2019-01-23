using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace InfoTransferContainers
{
    // internal static class Splitter
    // {
    //     public static List<string> trimThenHandlEmptyString(string str, char splitCh)
    //     {
    //         if (str.Trim().Length == 0) return new List<string>();
            
    //         // trim then split
    //         return str.Trim().Split(splitCh).ToList();
    //     }
    // }
    public class AvailableResourcesInfoContainer
    {
        
        public class AvailableResourceDescription
        {
            public long byteCount;
            public string versionStr;

            public AvailableResourceDescription(long byteCount, string versionStr)
            {
                this.byteCount = byteCount;
                this.versionStr = versionStr;
            }
        }
        public static AvailableResourcesInfoContainer deserialize(string str)
        {
            return JsonConvert.DeserializeObject<AvailableResourcesInfoContainer>(str);
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
            return JsonConvert.SerializeObject(this);
        }
    }

    public class ResourceVersionsInfoContainer
    {
        public class ResourceVersionDescription
        {
            public long byteCount;
            public ResourceVersionDescription(long byteCount)
            {
                this.byteCount = byteCount;
            }
        }
        
        public static ResourceVersionsInfoContainer deserialize(string str)
        {
            return JsonConvert.DeserializeObject<ResourceVersionsInfoContainer>(str);
        }

        public Dictionary<string, ResourceVersionDescription> versions;

        public string latestVersion;

        public ResourceVersionsInfoContainer(Dictionary<string, ResourceVersionDescription> versions, string latestVersion)
        {
            this.versions = versions;
            this.latestVersion = latestVersion;
        }

        public string serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class ResourceDependencyInfoContainer
    {
        public class DependencyDescription
        {
            public bool isDirectDependency;
            public string versionStr;
            public long byteCount;

            public DependencyDescription(string versionStr, bool isDirectDependency, long byteCount)
            {
                this.versionStr = versionStr;
                this.isDirectDependency = isDirectDependency;
                this.byteCount = byteCount;
            }
        }

        public static ResourceDependencyInfoContainer deserialize(string str)
        {
            return JsonConvert.DeserializeObject<ResourceDependencyInfoContainer>(str);
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
            return JsonConvert.SerializeObject(this);
        }
    }

    public class PublishDepsInfoContainer
    {
        public class PublishDepDescription
        {
            public string version;

            public PublishDepDescription(string version)
            {
                this.version = version;
            }
        }

        public Dictionary<string, PublishDepDescription> codeDeps;
        public Dictionary<string, PublishDepDescription> dataDeps;
        public Dictionary<string, PublishDepDescription> modelDeps;

        public static PublishDepsInfoContainer deserialize(string str)
        {
            return JsonConvert.DeserializeObject<PublishDepsInfoContainer>(str);
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
            return JsonConvert.SerializeObject(this);
        }
    }
}
