using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Ttc.DataAccess.Services;
using Ttc.WebApi.Utilities;

namespace Ttc.WebApi.Controllers
{
    [RoutePrefix("api/upload")]
    public class UploadController : BaseController
    {
        [HttpPost]
        [Route("Player")]
        public void UploadPlayerImage([FromBody]UploadPlayerImageDto data)
        {
            var file = GetServerPlayerImageFile(data.PlayerId);
            if (file.Exists)
            {
                var backupFile = GetServerImagePath(ImageFolder.Backup) + "\\";
                backupFile += "ply_" + data.PlayerId + "_" + Path.GetRandomFileName() + ".png";
                File.Move(file.FullName, backupFile);
            }
            
            string base64String = data.Image.Substring(22);
            byte[] bytes = Convert.FromBase64String(base64String);
            using (var ms = new MemoryStream(bytes))
            {
                var image = Image.FromStream(ms);
                image.Save(file.FullName);
            }
        }

        [HttpPost]
        [Route("Temp")]
        public async Task<HttpResponseMessage> UploadFile()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            string fullPath = GetServerImagePath(ImageFolder.Temp);
            var provider = new MultipartFormDataStreamProvider(fullPath);

            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);

                var file = provider.FileData.First();

                var localFileName = file.Headers.ContentDisposition.Name.Substring(1);
                localFileName = localFileName.Substring(0, localFileName.Length - 1);
                var originalFile = new FileInfo(localFileName);
                string localFileNameWithExt = Path.ChangeExtension(file.LocalFileName, originalFile.Extension);
                File.Move(file.LocalFileName, localFileNameWithExt);

                //string fieldType = provider.FormData.GetValues("uploadType").First();
                //file.Headers.ContentDisposition.Name
                //provider.FormData.GetValues(key)

                string publicFileName = "/img/temp/" + localFileNameWithExt.Replace(fullPath, "").Replace("\\", "");

                return Request.CreateResponse(HttpStatusCode.OK, new { fileName = publicFileName });
            }
            catch (System.Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        #region Private FileSystem stuff
        private static FileInfo GetServerPlayerImageFile(int playerId)
        {
            var path = GetServerImagePath(ImageFolder.Players);
            path += "\\" + playerId + ".png";
            return new FileInfo(path);
        }

        private static string GetServerImagePath(ImageFolder folder)
        {
            string root;
#if DEBUG
            root = Properties.Settings.Default.PublicImageFolder;
#else
            root  = HttpContext.Current.Server.MapPath("~/" + Properties.Settings.Default.PublicImageFolder);
#endif

            var fullPath = GetServerImagePath(root, folder);
            return fullPath;
        }

        private static string GetServerImagePath(string root, ImageFolder folder)
        {
            string fullPath;
            switch (folder)
            {
                case ImageFolder.Temp:
                    fullPath = root + "\\temp";
                    break;

                case ImageFolder.Backup:
                    fullPath = root + "\\backup";
                    break;

                case ImageFolder.Players:
                default:
                    fullPath = root + "\\players";
                    break;
            }

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            return fullPath;
        }

        private enum ImageFolder
        {
            Temp,
            Players,
            Backup
        }
        #endregion
    }

    public class UploadPlayerImageDto
    {
        /// <summary>
        /// Base64 encoded
        /// </summary>
        public string Image { get; set; }
        public int PlayerId { get; set; }

        public override string ToString()
        {
            return $"Image: {Image}, PlayerId: {PlayerId}";
        }
    }
}
