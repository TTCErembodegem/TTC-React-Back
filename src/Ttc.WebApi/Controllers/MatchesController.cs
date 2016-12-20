using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Http;
using Ttc.DataAccess.Services;
using Ttc.Model;
using Ttc.Model.Matches;
using Ttc.Model.Teams;
using Ttc.WebApi.Utilities;

namespace Ttc.WebApi.Controllers
{
    [RoutePrefix("api/matches")]
    public class MatchesController : BaseController
    {
        #region Constructor
        private readonly MatchService _service;
        private readonly TeamService _teamService;

        public MatchesController(MatchService service, TeamService teamService)
        {
            _service = service;
            _teamService = teamService;
        }
        #endregion

        #region Getters
        [AllowAnonymous]
        public IEnumerable<Match> Get()
        {
            var result = _service.GetMatches();
            CleanSensitiveData(result);
            return result;
        }

        [AllowAnonymous]
        public Match Get(int id)
        {
            var result = _service.GetMatch(id, true);
            CleanSensitiveData(result);
            return result;
        }

        [HttpGet]
        [Route("GetLastOpponentMatches")]
        [AllowAnonymous]
        public IEnumerable<OtherMatch> GetLastOpponentMatches(int teamId, int clubId, string teamCode)
        {
            var opponent = new OpposingTeam
            {
                ClubId = clubId,
                TeamCode = teamCode
            };
            var result = _service.GetLastOpponentMatches(teamId, opponent);
            return result;
        }
        #endregion



        #region Puts
        [HttpPost]
        [Route("FrenoyMatchSync")]
        [AllowAnonymous]
        public Match FrenoyMatchSync([FromBody]IdDto matchId)
        {
            var result = _service.FrenoyMatchSync(matchId.Id);
            CleanSensitiveData(result);
            return result;
        }

        [HttpPost]
        [Route("FrenoyTeamSync")]
        public void FrenoyTeamSync([FromBody]IdDto teamId)
        {
            _service.FrenoyTeamSync(teamId.Id);
        }

        [HttpPost]
        [Route("FrenoyOtherMatchSync")]
        [AllowAnonymous]
        public OtherMatch FrenoyOtherMatchSync([FromBody]IdDto matchId)
        {
            var result = _service.FrenoyOtherMatchSync(matchId.Id);
            return result;
        }

        [HttpPost]
        [Route("TogglePlayer")]
        public Match TogglePlayer([FromBody]MatchPlayer player)
        {
            var result = _service.ToggleMatchPlayer(player);
            return result;
        }

        [HttpPost]
        [Route("SetMyFormation")]
        public Match SetMyFormation([FromBody]MatchPlayer player)
        {
            var result = _service.SetMyFormation(player);
            return result;
        }

        [HttpPost]
        [Route("EditMatchPlayers")]
        public Match EditMatchPlayers([FromBody]MatchPlayersDto dto)
        {
            var result = _service.EditMatchPlayers(dto.MatchId, dto.PlayerIds, dto.NewStatus, dto.BlockAlso);
            return result;
        }

        [HttpPost]
        [Route("Report")]
        public Match Report([FromBody]MatchReport report)
        {
            var result = _service.UpdateReport(report);
            return result;
        }

        [HttpPost]
        [Route("Comment")]
        public Match Comment([FromBody]MatchComment comment)
        {
            var result = _service.AddComment(comment);
            return result;
        }

        [HttpPost]
        [Route("DeleteComment")]
        public Match DeleteComment([FromBody]IdDto comment)
        {
            var result = _service.DeleteComment(comment.Id);
            return result;
        }

        [HttpPost]
        [Route("UpdateScore")]
        public Match UpdateScore([FromBody]MatchScoreDto score)
        {
            if (score.Home < 0) score.Home = 0;
            else if (score.Home > 15) score.Home = 16;
            if (score.Out < 0) score.Out = 0;
            else if (score.Out > 15) score.Out = 16;

            var result = _service.UpdateScore(score.MatchId, new MatchScore(score.Home, score.Out));
            return result;
        }
        #endregion
    }

    public class MatchScoreDto
    {
        public int MatchId { get; set; }
        public int Home { get; set; }
        public int Out { get; set; }

        public override string ToString() => $"MatchId: {MatchId}, Home: {Home}, Out: {Out}";
    }

    public class IdDto
    {
        public int Id { get; set; } // oh boy
        public override string ToString() => Id.ToString();
    }

    public class MatchPlayersDto
    {
        public bool BlockAlso { get; set; }
        public int MatchId { get; set; }
        public string NewStatus { get; set; }
        public int[] PlayerIds { get; set; }

        public override string ToString()
        {
            return $"MatchId={MatchId}, Block={BlockAlso}, Status={NewStatus}, Players={string.Join(",", PlayerIds)}";
        }
    }
}
