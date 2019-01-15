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

// @TODO prevent circular dependencies
namespace RepositoryServer.Controllers
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
        public ActionResult<string> GetAvailableResources(string resourceType)
        {
            if (resourceType == "code") return serverDir.getCodeServerInfoDict().serialize();
            if (resourceType == "data") return serverDir.getDataServerInfoDict().serialize();
            if (resourceType == "models") return serverDir.getModelServerInfoDict().serialize();
            throw new FileNotFoundException();
        }

        [HttpGet("getresources/{resourceType}/{resourceName}/versions")]
        public ActionResult<string> GetResourceVersions(string resourceType, string resourceName)
        {
            if (resourceType == "code") return serverDir.getCodeVersions(resourceName).serialize();
            if (resourceType == "data") return serverDir.getDataVersions(resourceName).serialize();
            if (resourceType == "models") return serverDir.getModelVersions(resourceName).serialize();
            throw new FileNotFoundException();
        }

        [HttpGet("getresources/{resourceType}/{resourceName}/versions/{version}/file")]
        public FileResult GetResource(string resourceType, string resourceName, string version)
        {
            if (resourceType == "code") return serverDir.getCodeStream(resourceName, version);
            if (resourceType == "data") return serverDir.getDataStream(resourceName, version);
            if (resourceType == "models") return serverDir.getModelStream(resourceName, version);
            throw new FileNotFoundException();
        }

        [HttpGet("getresources/{resourceType}/{resourceName}/versions/{version}/dependencies")]
        public ActionResult<string> GetDependencies(string resourceType, string resourceName, string version)
        {
            if (resourceType == "code") return serverDir.getCodeResourceDeps(resourceName, version).serialize();
            if (resourceType == "data") return serverDir.getDataResourceDeps(resourceName, version).serialize();
            if (resourceType == "models") return serverDir.getModelResourceDeps(resourceName, version).serialize();
            throw new FileNotFoundException();
        }

        // @TODO locks for uploading files? In order to prevent conflicting uploads
        [HttpPost("postresources/{resourceType}/{resourceName}/versions/{version}/post")]
        public ActionResult UploadImageAndOpenIt(
            string resourceType,
            string resourceName,
            string version,
            [FromQuery] string deps)
        {
            PublishDepsInfoContainer depsInfo = PublishDepsInfoContainer.deserialize(deps);

            Stream requestFileStream = Request.Body;

            // if (resourceType == "code") return serverDir
            if (resourceType == "code") serverDir.addCodeResource(resourceName, version, depsInfo, requestFileStream);
            if (resourceType == "data") serverDir.addDataResource(resourceName, version, depsInfo, requestFileStream);
            if (resourceType == "models") serverDir.addModelResource(resourceName, version, depsInfo, requestFileStream);

            return Ok();
        }
    }
}
