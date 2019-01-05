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


    public class DepsTransferContainer
    {
        public class DepDescription
        {
            public string versionStr;

            public DepDescription(string versionStr)
            {
                this.versionStr = versionStr;
            }
        }

        public Dictionary<string, DepDescription> codeDeps;
        public Dictionary<string, DepDescription> dataDeps;
        public Dictionary<string, DepDescription> modelDeps;

        public DepsTransferContainer(
            Dictionary<string, DepDescription> codeDeps,
            Dictionary<string, DepDescription> dataDeps,
            Dictionary<string, DepDescription> modelDeps)
        {
            this.codeDeps = codeDeps;
            this.dataDeps = dataDeps;
            this.modelDeps = modelDeps;
        }
    }
}
