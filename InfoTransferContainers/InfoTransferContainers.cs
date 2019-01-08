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
            public string versionStr;
            public long byteCount;

            public DependencyDescription(string versionStr)
            {
                this.versionStr = versionStr;
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
    }
}
