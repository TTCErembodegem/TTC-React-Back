using System.Collections.Generic;
using System.Web.Http;
using Ttc.DataAccess.Services;
using Ttc.Model.Matches;
using Ttc.Model.Teams;
using Ttc.WebApi.Utilities;

namespace Ttc.WebApi.Controllers
{
    [RoutePrefix("api/matches")]
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

        [Route("GetRelevantMatches")]
        public IEnumerable<Match> GetRelevantMatches() => _service.GetRelevantMatches();

        [HttpPost]
        public Match TogglePlayer([FromBody]MatchPlayer player)
        {
            var result = _service.ToggleMatchPlayer(player);
            return result;
        }

        [HttpGet]
        [Route("GetLastOpponentMatches")]
        public IEnumerable<Match> GetLastOpponentMatches(string teamCode, int clubId)
        {
            var opponent = new OpposingTeam
            {
                ClubId = clubId,
                TeamCode = teamCode
            };
            return _service.GetLastOpponentMatches(opponent);
        }
    }
}
