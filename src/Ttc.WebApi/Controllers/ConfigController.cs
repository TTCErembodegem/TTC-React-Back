using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ttc.DataAccess.Services;
using Ttc.Model.Core;

namespace Ttc.WebApi.Controllers;

[Authorize]
[Route("api/config")]
public class ConfigController
{
    #region Constructor
    private readonly ConfigService _service;
    private readonly TtcLogger _logger;

    public ConfigController(ConfigService service, TtcLogger logger)
    {
        _service = service;
        _logger = logger;
    }
    #endregion

    [HttpGet]
    [AllowAnonymous]
    public async Task<Dictionary<string, string>> Get()
    {
        _logger.Information("Getting config");
        return await _service.Get();
    }

    [HttpPost]
    public async Task Post([FromBody] ConfigParam param)
    {
        await _service.Save(param.Key, param.Value);
    }

    [HttpPost]
    [Route("Log")]
    [AllowAnonymous]
    public void Log([FromBody] dynamic context)
    {
        var str = context.args.ToString();
        _logger.Error(str);
    }

    [HttpGet]
    [Route("Log/Get")]
    [AllowAnonymous]
    public HttpResponseMessage GetLogging()
    {
        //FileAppender rootAppender = ((Hierarchy)LogManager.GetRepository())
        //    .Root.Appenders.OfType<FileAppender>()
        //    .FirstOrDefault();

        var resp = new HttpResponseMessage(HttpStatusCode.OK);

        //if (rootAppender == null)
        //{
        //    resp.Content = new StringContent("No log4net FileAppender configured!", System.Text.Encoding.UTF8, "text/plain");
        //}
        //else
        //{
        //    resp.Content = new StringContent(System.IO.File.ReadAllText(rootAppender.File), System.Text.Encoding.UTF8, "text/plain");
        //}
        return resp;
    }
}

public class ConfigParam
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";

    public override string ToString() => $"{Key} => {Value}";
}

