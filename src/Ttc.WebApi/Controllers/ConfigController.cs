using Ttc.DataAccess.Services;
using Ttc.WebApi.Utilities;

namespace Ttc.WebApi.Controllers
{
    public class ConfigController : BaseController
    {
        #region Constructor
        private readonly ConfigService _service;

        public ConfigController(ConfigService service)
        {
            _service = service;
        }
        #endregion

        public object Get() => _service.Get();
    }
}
