using System;
using System.Collections.Generic;

namespace InfoTransferContainers
{
    public class FileInfoTransferContainer
    {
        public long byteCount;
        public string versionStr;

        public FileInfoTransferContainer(long byteCount, string versionStr)
        {
            this.byteCount = byteCount;
            this.versionStr = versionStr;
        }
    }

    public class ResourceVersionsInfoContainer
    {
        public List<string> versions;
        public string latestVersion;

        public ResourceVersionsInfoContainer(List<string> versions, string latestVersion)
        {
            this.versions = versions;
            this.latestVersion = latestVersion;
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
    }
}
