using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
// using ZmodFiles;
using System.IO;
using InfoTransferContainers;
using ServerResourceDirNS;
using System.Web.Http;

namespace example_server.Controllers
{
    [Route("api/resources")]
    [ApiController]
    public class ResourcesController : ControllerBase
    {
        private ServerResourceDir serverDir = new ServerResourceDir(
            Path.Join(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
                "ZMODServerFiles"
            )
        );

        [HttpGet("{resourceType}")]
        public ActionResult<Dictionary<string, FileInfoTransferContainer>> GetAvailableResources(string resourceType)
        {
            if (resourceType == "code") return serverDir.getCodeServerInfoDict();
            if (resourceType == "data") return serverDir.getDataServerInfoDict();
            if (resourceType == "model") return serverDir.getModelServerInfoDict();
            throw new FileNotFoundException();
        }

        [HttpGet("{resourceType}/{resourceName}/versions")]
        public ActionResult<ResourceVersionsInfoContainer> GetResourceVersions(string resourceType, string resourceName)
        {
            if (resourceType == "code") return serverDir.getCodeVersions(resourceName);
            if (resourceType == "data") return serverDir.getDataVersions(resourceName);
            if (resourceType == "model") return serverDir.getModelVersions(resourceName);
            throw new FileNotFoundException();
        }

        [HttpGet("{resourceType}/{resourceName}/versions/{version}/file")]
        public FileResult GetResource(string resourceType, string resourceName, string version)
        {
            if (resourceType == "code") return serverDir.getCodeStream(resourceName, version);
            if (resourceType == "data") return serverDir.getDataStream(resourceName, version);
            if (resourceType == "model") return serverDir.getModelStream(resourceName, version);
            throw new FileNotFoundException();
        }

        [HttpGet("{resourceType}/{resourceName}/versions/{version}/dependencies")]
        public ActionResult<ResourceDependencyInfoContainer> GetDependencies(string resourceType, string resourceName, string version)
        {
            System.Console.WriteLine("=================================================");
            System.Console.WriteLine(resourceType);
            System.Console.WriteLine(resourceName);
            System.Console.WriteLine(version);
            if (resourceType == "code") return serverDir.getCodeResourceDeps(resourceName, version);
            if (resourceType == "data") return serverDir.getDataResourceDeps(resourceName, version);
            if (resourceType == "model") return serverDir.getModelResourceDeps(resourceName, version);
            throw new FileNotFoundException();
        }
    }
}
