using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Cors;
using Ttc.DataAccess;
using Ttc.DataAccess.Services;
using Ttc.Model.Matches;

namespace Ttc.WebApi.Controllers
{
    //public class SelectMatchPlayer
    //{
    //    public int MatchId { get; set; }
    //    public int PlayerId { get; set; }
    //}

    [EnableCors("*", "*", "*")]
    public class MatchesController : ApiController
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
        public MatchPlayer AddPlayer([FromBody]MatchPlayer player)
        {
            return player;
            //var matchId = selectPlayer.MatchId;
            //var playerId = selectPlayer.PlayerId;
            //var match = _service.GetMatch(matchId);
            //var team = _teamService.GetTeam(match.TeamId);
            //var player = new PlayerService().GetPlayer(playerId);
            //var matchPlayer = match.Report.Players.SingleOrDefault(x => x.PlayerId == playerId);
            //if (matchPlayer == null)
            //{
            //    var comp = player.GetCompetition(team.Competition);
            //    var newPlayer = new MatchPlayer
            //    {
            //        Home = true,
            //        PlayerId = playerId,
            //        Position = match.Report.Players.Count + 1,
            //        Ranking = comp.Ranking,
            //        UniqueIndex = comp.UniqueIndex,
            //        Name = player.Alias
            //    };
            //    return _service.AddMatchPlayer(newPlayer);
            //}
            //else
            //{
            //    _service.DeleteMatchPlayer(matchPlayer);
            //    return null;
            //}
        }
    }
}
