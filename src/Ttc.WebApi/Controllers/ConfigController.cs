using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using Ttc.DataAccess.Services;
using Ttc.WebApi.Utilities;

namespace Ttc.WebApi.Controllers
{
    [RoutePrefix("api/config")]
    public class ConfigController : BaseController
    {
        #region Constructor
        private readonly ConfigService _service;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ConfigController));

        public ConfigController(ConfigService service)
        {
            _service = service;
        }
        #endregion

        //[AllowAnonymous]
        //public object Get() => _service.Get();

        [HttpPost]
        [Route("Log")]
        [AllowAnonymous]
        public void Log([FromBody]dynamic context)
        {
            var str = context.args.ToString();
            Logger.Error(str);
        }

        [HttpGet]
        [Route("Log/Get")]
        [AllowAnonymous]
        public HttpResponseMessage GetLogging()
        {
            FileAppender rootAppender = ((Hierarchy)LogManager.GetRepository())
                .Root.Appenders.OfType<FileAppender>()
                .FirstOrDefault();

            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            
            if (rootAppender == null)
            {
                resp.Content = new StringContent("No log4net FileAppender configured!", System.Text.Encoding.UTF8, "text/plain");
            }
            else
            {
                resp.Content = new StringContent(System.IO.File.ReadAllText(rootAppender.File), System.Text.Encoding.UTF8, "text/plain");
            }
            return resp;
        }
    }
}
