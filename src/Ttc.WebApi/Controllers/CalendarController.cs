using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Cors;
using Ttc.DataAccess;
using Ttc.DataAccess.Services;
using Ttc.Model.Matches;
using Ttc.WebApi.Utilities;

namespace Ttc.WebApi.Controllers
{
    public class MatchesController : BaseController
    {
        #region Constructor
        private readonly CalendarService _service;
        private readonly TeamService _teamService;

        public MatchesController(CalendarService service, TeamService teamService)
        {
            _service = service;
            _teamService = teamService;
        }
        #endregion

        public IEnumerable<Match> Get() => _service.GetRelevantMatches();

        [HttpPost]
        public Match TogglePlayer([FromBody]MatchPlayer player)
        {
            var result = _service.ToggleMatchPlayer(player);
            return result;
        }
    }
}
