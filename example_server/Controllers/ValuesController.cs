using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using InfoTransferContainers;
using ServerResourceDirNS;
using System.Web.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.Web;
using Newtonsoft.Json;

namespace example_server.Controllers
{
    [Route("api")]
    [ApiController]
    public class ResourcesController : ControllerBase
    {
        private ServerResourceDir serverDir = new ServerResourceDir(
            Path.Join(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
                "ZMODServerFiles"
            )
        );

        [HttpGet("getresources/{resourceType}")]
        public ActionResult<Dictionary<string, FileInfoTransferContainer>> GetAvailableResources(string resourceType)
        {
            if (resourceType == "code") return serverDir.getCodeServerInfoDict();
            if (resourceType == "data") return serverDir.getDataServerInfoDict();
            if (resourceType == "model") return serverDir.getModelServerInfoDict();
            throw new FileNotFoundException();
        }

        [HttpGet("getresources/{resourceType}/{resourceName}/versions")]
        public ActionResult<ResourceVersionsInfoContainer> GetResourceVersions(string resourceType, string resourceName)
        {
            if (resourceType == "code") return serverDir.getCodeVersions(resourceName);
            if (resourceType == "data") return serverDir.getDataVersions(resourceName);
            if (resourceType == "model") return serverDir.getModelVersions(resourceName);
            throw new FileNotFoundException();
        }

        [HttpGet("getresources/{resourceType}/{resourceName}/versions/{version}/file")]
        public FileResult GetResource(string resourceType, string resourceName, string version)
        {
            if (resourceType == "code") return serverDir.getCodeStream(resourceName, version);
            if (resourceType == "data") return serverDir.getDataStream(resourceName, version);
            if (resourceType == "model") return serverDir.getModelStream(resourceName, version);
            throw new FileNotFoundException();
        }

        [HttpGet("getresources/{resourceType}/{resourceName}/versions/{version}/dependencies")]
        public ActionResult<ResourceDependencyInfoContainer> GetDependencies(string resourceType, string resourceName, string version)
        {
            if (resourceType == "code") return serverDir.getCodeResourceDeps(resourceName, version);
            if (resourceType == "data") return serverDir.getDataResourceDeps(resourceName, version);
            if (resourceType == "model") return serverDir.getModelResourceDeps(resourceName, version);
            throw new FileNotFoundException();
        }

        // @TODO locks for uploading files? In order to prevent conflicting uploads
        [HttpPost("postresources/{resourceType}/{resourceName}/versions/{version}/post")]
        public ActionResult UploadImageAndOpenIt(
            string resourceType,
            string resourceName,
            string version,
            [FromQuery] string deps,
            [FromQuery] string dataDeps,
            [FromQuery] string modelDeps)
        {
            System.Console.WriteLine("RECEIVED VALUES");
            System.Console.WriteLine(resourceType);
            System.Console.WriteLine(resourceName);
            System.Console.WriteLine(version);
            System.Console.WriteLine(deps);

            PublishDepsInfoContainer depsInfo = JsonConvert.DeserializeObject<PublishDepsInfoContainer>(deps);
            
            Stream requestFileStream = Request.Body;

            // if (resourceType == "code") return serverDir
            if (resourceType == "code") serverDir.addCodeResource(resourceName, version, depsInfo, requestFileStream);
            if (resourceType == "data") serverDir.addDataResource(resourceName, version, depsInfo, requestFileStream);
            if (resourceType == "model") serverDir.addModelResource(resourceName, version, depsInfo, requestFileStream);
            
            return Ok();
        }
    }
}
