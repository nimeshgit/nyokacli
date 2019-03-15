using FSOpsNS;
using CLIInterfaceNS;

namespace NyokaRemoteNS
{
    class NyokaRemote 
    {
        public string RepositoryServer { get; set;}
        public string ZementisServer {get; set;}
        public string ZementisModeler {get; set;}

        public void Save()
        {

        }
        
    }
    class NyokaRemoteInfo
    {
        private static NyokaRemoteInfo _instance;
        protected NyokaRemoteInfo()
        {

        }
        public static NyokaRemoteInfo Instance()
        {
            if (_instance == null)
            {
                _instance = new NyokaRemoteInfo();
            }
            return _instance;
        }
        
    }
}