using System;
using JsonPitCore;
using HDitem.Persist;

namespace NyokaServerConfiguration
{
    public class NyokaRemoteInfo : JsonPitCore.Item
    {
        public string RepositoryServer { get; set; }
        public string ZementisServer { get; set; }
        public string ZementisModeler { get; set; } 
    }
}