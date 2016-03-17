using System.Web.Http;
using Ttc.DataAccess.Services;
using Ttc.WebApi.Utilities;

namespace Ttc.WebApi.Controllers
{
    [RoutePrefix("api/config")]
    public class ConfigController : BaseController
    {
        #region Constructor
        private readonly ConfigService _service;

        public ConfigController(ConfigService service)
        {
            _service = service;
        }
        #endregion

        [AllowAnonymous]
        public object Get() => _service.Get();
    }
}
