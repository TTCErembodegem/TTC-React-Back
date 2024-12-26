using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ttc.DataAccess.Services;
using Ttc.Model.Core;
using Ttc.WebApi.Utilities;

namespace Ttc.WebApi.Controllers;

[Authorize]
[Route("api/upload")]
public class UploadController
{
    private readonly TtcSettings _settings;

    public UploadController(TtcSettings settings)
    {
        _settings = settings;
    }

    [HttpPost]
    [Route("Image")]
    public void UploadImage([FromBody] UploadImageDto data)
    {
        var file = GetServerPlayerImageFile(data.Type, data.DataId);
        if (file.Exists)
        {
            var backupFile = GetServerImagePath(ImageFolder.Backup) + "\\";
            backupFile += data.Type.Replace("-", "_") + "_" + data.DataId + "_" + Path.GetRandomFileName() + ".png";
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
    public async Task<HttpResponseMessage> UploadTempFile()
    {
        //if (!Request.Content.IsMimeMultipartContent())
        //{
        //    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        //}

        //string fullPath = GetServerImagePath(ImageFolder.Temp);
        //var provider = new MultipartFormDataStreamProvider(fullPath);

        //try
        //{
        //    await Request.Content.ReadAsMultipartAsync(provider);

        //    var file = provider.FileData.First();

        //    var localFileName = file.Headers.ContentDisposition.Name.Substring(1);
        //    localFileName = localFileName.Substring(0, localFileName.Length - 1);
        //    var originalFile = new FileInfo(localFileName);
        //    string localFileNameWithExt = Path.ChangeExtension(file.LocalFileName, originalFile.Extension);
        //    File.Move(file.LocalFileName, localFileNameWithExt);

        //    string fieldType = provider.FormData.GetValues("uploadType").First();
        //    if (fieldType == "match")
        //    {
        //        var matchPath = GetServerImagePath(ImageFolder.Match);
        //        var fn = new FileInfo(localFileNameWithExt);
        //        string matchName = matchPath + "\\" + fn.Name;
        //        File.Move(localFileNameWithExt, matchName);
        //        return Request.CreateResponse(HttpStatusCode.OK, new { fileName = "/img/matches/" + fn.Name });
        //    }

        //    string publicFileName = "/img/temp/" + localFileNameWithExt.Replace(fullPath, "").Replace("\\", "");

        //    return Request.CreateResponse(HttpStatusCode.OK, new { fileName = publicFileName });
        //}
        //catch (System.Exception e)
        //{
        //    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
        //}
        return null;
    }

    #region Private FileSystem stuff
    private FileInfo GetServerPlayerImageFile(string type, int playerId)
    {
        var path = GetServerImagePath(ImageFolder.Players);
        if (type == "player-photo")
        {
            path += "\\" + playerId + ".png";
        }
        else
        {
            path += "\\" + playerId + "_avatar.png";
        }

        return new FileInfo(path);
    }

    private string GetServerImagePath(ImageFolder folder)
    {
        var fullPath = GetServerImagePath(_settings.PublicImageFolder, folder);
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

            case ImageFolder.Match:
                fullPath = root + "\\matches";
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
        Backup,
        Match
    }
    #endregion
}

public class UploadImageDto
{
    /// <summary>
    /// Base64 encoded
    /// </summary>
    public string Image { get; set; }
    public int DataId { get; set; }
    public string Type { get; set; }

    public override string ToString() => $"Image: {Image}, Id: {DataId}";
}
