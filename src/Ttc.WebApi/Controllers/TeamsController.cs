using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ttc.DataAccess.Services;
using Ttc.Model;
using Ttc.Model.Teams;

namespace Ttc.WebApi.Controllers
{
    public class TeamsController : ApiController
    {
        #region Constructor
        private readonly TeamService _service;

        public TeamsController(TeamService service)
        {
            _service = service;
        }
        #endregion

        public IEnumerable<Team> Get()
        {
            return _service.GetForCurrentYear();
        }
    }
}
