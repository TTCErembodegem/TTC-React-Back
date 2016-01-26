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
    public class CalendarController : ApiController
    {
        #region Constructor
        private readonly CalendarService _service;

        public CalendarController(CalendarService service)
        {
            _service = service;
        }
        #endregion

        public IEnumerable<Match> Get() => _service.GetRelevantCalendarItems();
    }
}
