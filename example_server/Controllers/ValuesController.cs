using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ZmodFiles;
using System.IO;

namespace example_server.Controllers
{
    [Route("api/resources")]
    [ApiController]
    public class ResourcesController : ControllerBase
    {
        private static List<T> concatEnumerables<T>(List<IEnumerable<T>> valEnumerables) {
            List<T> concatenated = new List<T>();
            foreach (IEnumerable<T> valEnumerable in valEnumerables) {
                concatenated.AddRange(valEnumerable);
            }
            return concatenated;
        }
        private ZmodDirectory zmodDir = new ZmodDirectory("~/DropboxZMOD/ZMOD/");
        
        [HttpGet("code")]
        public ActionResult<IEnumerable<string>> CodeGet() {
            return concatEnumerables<string>(new List<IEnumerable<string>>() {
                zmodDir.PyFiles.Keys,
                zmodDir.IpynbFiles.Keys,
            });
        }

        [HttpGet("data")]
        public ActionResult<IEnumerable<string>> DataGet() {
            return concatEnumerables<string>(new List<IEnumerable<string>>() {
                zmodDir.CsvFiles.Keys,
                zmodDir.JsonFiles.Keys,
                zmodDir.ImageFiles.Keys
            });
        }

        [HttpGet("model")]
        public ActionResult<IEnumerable<string>> ModelGet() {
            return zmodDir.PmmlFiles.Keys;
        }

        [HttpGet("code/{resourceName}")]
        public FileResult GetCodeResource(string resourceName) {
            if (zmodDir.PyFiles.ContainsKey(resourceName)) {
                FileStream fileStream = zmodDir.PyFiles[resourceName].info.OpenRead();
                return new FileStreamResult(fileStream, "application/x-python-code");
            } else if (zmodDir.IpynbFiles.ContainsKey(resourceName)) {
                FileStream fileStream = zmodDir.IpynbFiles[resourceName].info.OpenRead();
                return new FileStreamResult(fileStream, "application/x-ipynb+json");
            } else {
                // @TODO does throwing an exception expose inside of server too much?
                throw new FileNotFoundException();
            }
        }

        [HttpGet("data/{resourceName}")]
        public FileResult GetDataResource(string resourceName) {
            if (zmodDir.CsvFiles.ContainsKey(resourceName)) {
                FileStream fileStream = zmodDir.CsvFiles[resourceName].info.OpenRead();
                return new FileStreamResult(fileStream, "text/csv");
            } else if (zmodDir.JsonFiles.ContainsKey(resourceName)) {
                FileStream fileStream = zmodDir.JsonFiles[resourceName].info.OpenRead();
                return new FileStreamResult(fileStream, "application/json");
            } else if (zmodDir.ImageFiles.ContainsKey(resourceName)) {
                FileInfo info = zmodDir.ImageFiles[resourceName].info;
                FileStream fileStream = info.OpenRead();
                
                if (info.Extension == ".png") {
                    return new FileStreamResult(fileStream, "image/png");
                } else if (info.Extension == ".jpg") {
                    return new FileStreamResult(fileStream, "image/jpeg");
                } else {
                    throw new FileNotFoundException();
                }
            } else {
                throw new FileNotFoundException();
            }
        }

        [HttpGet("model/{resourceName}")]
        public FileResult GetModelResource(string resourceName) {
            if (zmodDir.PmmlFiles.ContainsKey(resourceName)) {
                FileStream fileStream = zmodDir.PmmlFiles[resourceName].info.OpenRead();

                // @QUESTION should this be text/xml?
                return new FileStreamResult(fileStream, "application/xml");
            } else {
                throw new FileNotFoundException();
            }
        }
    }
}
