using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using Ttc.DataAccess;
using Ttc.DataAccess.Services;

namespace Ttc.WebApi.Controllers
{
    public class ConfigController : ApiController
    {
        #region Constructor
        private readonly ConfigService _configService;

        public ConfigController(ConfigService configService)
        {
            _configService = configService;
        }
        #endregion

        public object Get()
        {
            return _configService.Get();
        }
    }
}
