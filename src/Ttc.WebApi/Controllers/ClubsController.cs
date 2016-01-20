using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Ttc.DataAccess;
using Ttc.DataAccess.Services;
using Ttc.Model;
using Ttc.Model.Clubs;

namespace Ttc.WebApi.Controllers
{
    public class ClubsController : ApiController
    {
        #region Constructor
        private readonly ClubService _service;

        public ClubsController(ClubService service)
        {
            _service = service;
        }
        #endregion

        public IEnumerable<Club> Get()
        {
            return _service.GetActiveClubs();
        }
    }
}
