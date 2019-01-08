using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
// using ZmodFiles;
using System.IO;
using InfoTransferContainers;
using ServerResourceDirNS;

namespace example_server.Controllers
{
    [Route("api/resources")]
    [ApiController]
    public class ResourcesController : ControllerBase
    {
        private static List<T> concatEnumerables<T>(List<IEnumerable<T>> valEnumerables)
        {
            List<T> concatenated = new List<T>();
            
            foreach (IEnumerable<T> valEnumerable in valEnumerables)
            {
                concatenated.AddRange(valEnumerable);
            }
            
            return concatenated;
        }
        
        private static Dictionary<K, V> mergeDictsUnsafe<K, V>(params Dictionary<K, V>[] dicts)
        {
            Dictionary<K, V> mergedDict = new Dictionary<K, V>();

            foreach (Dictionary<K, V> dict in dicts)
            {
                foreach (KeyValuePair<K, V> entry in dict)
                {
                    mergedDict[entry.Key] = entry.Value;
                }
            }

            return mergedDict;
        }

        private ServerResourceDir serverDir = new ServerResourceDir(
            Path.Join(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
                "ZMODServerFiles"
            )
        );
        
        [HttpGet("code")]
        public ActionResult<Dictionary<string, FileInfoTransferContainer>> CodeGet()
        {
            return serverDir.getCodeServerInfoDict();
        }

        [HttpGet("data")]
        public ActionResult<Dictionary<string, FileInfoTransferContainer>> DataGet()
        {
            return serverDir.getDataServerInfoDict();
        }

        [HttpGet("model")]
        public ActionResult<Dictionary<string, FileInfoTransferContainer>> ModelGet()
        {
            return serverDir.getModelServerInfoDict();
        }

        [HttpGet("code/{resourceName}/{version}")]
        public FileResult GetCodeResource(string resourceName, string version)
        {
            if (!serverDir.getCodeServerInfoDict().Keys.Contains(resourceName))
            {
                throw new FileNotFoundException();
            }

            return serverDir.getCodeStream(resourceName, version);
        }

        [HttpGet("data/{resourceName}/{version}")]
        public FileResult GetDataResource(string resourceName, string version)
        {
            if (!serverDir.getDataServerInfoDict().Keys.Contains(resourceName))
            {
                throw new FileNotFoundException();
            }

            return serverDir.getDataStream(resourceName, version);
        }

        [HttpGet("model/{resourceName}/{version}")]
        public FileResult GetModelResource(string resourceName, string version)
        {
            if (!serverDir.getModelServerInfoDict().Keys.Contains(resourceName))
            {
                throw new FileNotFoundException();
            }

            return serverDir.getModelStream(resourceName, version);
        }

        [HttpGet("code/{resourceName}/{version}/dependencies")]
        public ActionResult<ResourceInfoContainer> GetcodeDependencies(string resourceName, string version)
        {
            return serverDir.getCodeResourceDeps(resourceName, version);
        }
        
        [HttpGet("data/{resourceName}/{version}/dependencies")]
        public ActionResult<ResourceInfoContainer> GetdataDependencies(string resourceName, string version)
        {
            return serverDir.getDataResourceDeps(resourceName, version);
        }
        
        [HttpGet("model/{resourceName}/{version}/dependencies")]
        public ActionResult<ResourceInfoContainer> GetmodelDependencies(string resourceName, string version)
        {
            return serverDir.getModelResourceDeps(resourceName, version);
        }
    }
}
