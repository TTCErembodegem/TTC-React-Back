using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ttc.DataAccess;

namespace Ttc.WebApi.Controllers
{
    public class PlayersController : ApiController
    {
        public IHttpActionResult Get()
        {
            using (var x = new TtcDbContext())
            {
                return Json(x.Spelers);
            }
        }
    }
}
