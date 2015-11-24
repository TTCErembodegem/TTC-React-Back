using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using Ttc.DataAccess;

namespace Ttc.WebApi.Controllers
{
    public class ConfigController : ApiController
    {
        public string Get()
        {
            using (var x = new TtcDbContext())
                return x.Spelers.First().Naam;
        }
    }
}
