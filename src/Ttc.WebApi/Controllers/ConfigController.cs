using System.Web.Http;
using log4net;
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

        [AllowAnonymous]
        public object Get() => _service.Get();

        [HttpPost]
        [Route("Log")]
        [AllowAnonymous]
        public void Log([FromBody]dynamic context)
        {
            var str = context.args.ToString();
            Logger.Error(str);
        }
    }
}
