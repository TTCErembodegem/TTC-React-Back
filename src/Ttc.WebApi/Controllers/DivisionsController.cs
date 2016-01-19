using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ttc.DataAccess.Services;
using Ttc.Model;

namespace Ttc.WebApi.Controllers
{
    public class DivisionsController : ApiController
    {
        #region Constructor
        private readonly DivisionService _service;

        public DivisionsController(DivisionService service)
        {
            _service = service;
        }
        #endregion

        public IEnumerable<Division> Get()
        {
            return _service.GetForCurrentYear();
        }
    }
}
