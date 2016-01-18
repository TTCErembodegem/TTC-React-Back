using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Ttc.DataAccess;
using Ttc.DataAccess.Services;
using Ttc.Model;

namespace Ttc.WebApi.Controllers
{
    public class PlayersController : ApiController
    {
        public IEnumerable<Player> Get()
        {
            var ps = new PlayerService();
            return ps.Get();
        }
    }
}
