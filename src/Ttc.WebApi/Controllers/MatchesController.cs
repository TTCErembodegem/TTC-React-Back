using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Ttc.DataAccess.Services;
using Ttc.Model;
using Ttc.Model.Matches;
using Ttc.Model.Teams;
using Ttc.WebApi.Emailing;
using Ttc.WebApi.Utilities;

namespace Ttc.WebApi.Controllers
{
    [RoutePrefix("api/matches")]
    public class MatchesController : BaseController
    {
        #region Constructor
        private readonly MatchService _service;
        private readonly PlayerService _playerService;
        private readonly ConfigService _configService;

        public MatchesController(MatchService service, PlayerService playerService, ConfigService configService)
        {
            _service = service;
            _playerService = playerService;
            _configService = configService;
        }
        #endregion

        #region Getters
        [AllowAnonymous]
        public async Task<IEnumerable<Match>> Get()
        {
            var result = await _service.GetMatches();
            CleanSensitiveData(result);
            return result;
        }

        [AllowAnonymous]
        public async Task<Match> Get(int id)
        {
            var result = await _service.GetMatch(id, true);
            CleanSensitiveData(result);
            return result;
        }

        [HttpGet]
        [Route("GetOpponentMatches")]
        [AllowAnonymous]
        public async Task<IEnumerable<OtherMatch>> GetOpponentMatches(int teamId, int? clubId = null, string teamCode = null)
        {
            // This is also called from Team Week display where there is no opponent
            var opponent = clubId.HasValue ? OpposingTeam.Create(clubId, teamCode) : null;
            var result = await _service.GetOpponentMatches(teamId, opponent);
            CleanSensitiveData(result);
            return result;
        }
        #endregion



        #region Puts
        [HttpPost]
        [Route("FrenoyMatchSync")]
        [AllowAnonymous]
        public async Task<Match> FrenoyMatchSync([FromBody]IdDto matchId, [FromUri]bool forceSync)
        {
            var result = await _service.FrenoyMatchSync(matchId.Id, forceSync);
            CleanSensitiveData(result);
            return result;
        }

        [HttpPost]
        [Route("FrenoyTeamSync")]
        public async Task FrenoyTeamSync([FromBody]IdDto teamId)
        {
            await _service.FrenoyTeamSync(teamId.Id);
        }

        [HttpPost]
        [Route("FrenoyOtherMatchSync")]
        [AllowAnonymous]
        public async Task<OtherMatch> FrenoyOtherMatchSync([FromBody]IdDto matchId)
        {
            var result = await _service.FrenoyOtherMatchSync(matchId.Id);
            return result;
        }

        [HttpPost]
        [Route("TogglePlayer")]
        public async Task<Match> TogglePlayer([FromBody]MatchPlayer player)
        {
            var result = await _service.ToggleMatchPlayer(player);
            return result;
        }

        [HttpPost]
        [Route("SetMyFormation")]
        public async Task<Match> SetMyFormation([FromBody]MatchPlayer player)
        {
            var result = await _service.SetMyFormation(player);
            return result;
        }

        [HttpPost]
        [Route("EditMatchPlayers")]
        public async Task<Match> EditMatchPlayers([FromBody]MatchPlayersDto dto)
        {
            var result = await _service.EditMatchPlayers(dto.MatchId, dto.PlayerIds, dto.NewStatus, dto.BlockAlso, dto.Comment);
            return result;
        }

        [HttpPost]
        [Route("Report")]
        public async Task<Match> Report([FromBody]MatchReport report)
        {
            var result = await _service.UpdateReport(report);
            return result;
        }

        [HttpPost]
        [Route("Comment")]
        public async Task<Match> Comment([FromBody]MatchComment comment)
        {
            var result = await _service.AddComment(comment);
            return result;
        }

        [HttpPost]
        [Route("DeleteComment")]
        public async Task<Match> DeleteComment([FromBody]IdDto comment)
        {
            var result = await _service.DeleteComment(comment.Id);
            return result;
        }

        [HttpPost]
        [Route("UpdateScore")]
        public async Task<Match> UpdateScore([FromBody]MatchScoreDto score)
        {
            if (score.Home < 0) score.Home = 0;
            else if (score.Home > 15) score.Home = 16;
            if (score.Out < 0) score.Out = 0;
            else if (score.Out > 15) score.Out = 16;

            var result = await _service.UpdateScore(score.MatchId, new MatchScore(score.Home, score.Out));
            return result;
        }
        #endregion


        [HttpGet]
        [Route("ExcelScoresheet/{matchId}")]
        public async Task<string> GetExcelExport([FromUri] int matchId)
        {
            var result = await _service.GetExcelExport(matchId);
            return Convert.ToBase64String(result.Item1);
        }

        [HttpPost]
        [Route("WeekCompetitionEmail")]
        public async Task WeekCompetitionEmail([FromBody] WeekCompetitionEmailModel email)
        {
            var emailConfig = await _configService.GetEmailConfig();
            var players = await _playerService.GetOwnClub();
            var playerEmails = players.Select(player => player.Contact.Email);
            EmailService.SendEmail(playerEmails, email.Title, email.Email, emailConfig).Wait();
            //EmailService.SendEmail("wouter@pongit.be", email.Title, email.Email, emailConfig).Wait();
        }
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
        public string Comment { get; set; }

        public override string ToString() => $"MatchId={MatchId}, Block={BlockAlso}, Status={NewStatus}, Players={string.Join(",", PlayerIds)}";
    }
}
