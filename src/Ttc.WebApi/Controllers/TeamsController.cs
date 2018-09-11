using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Ttc.DataAccess.Services;
using Ttc.Model.Teams;
using Ttc.WebApi.Utilities;

namespace Ttc.WebApi.Controllers
{
    [RoutePrefix("api/teams")]
    public class TeamsController : BaseController
    {
        #region Constructor
        private readonly TeamService _service;

        public TeamsController(TeamService service)
        {
            _service = service;
        }
        #endregion

        [AllowAnonymous]
        public async Task<IEnumerable<Team>> Get() => await _service.GetForCurrentYear();

        [AllowAnonymous]
        public async Task<Team> Get(int id) => await _service.GetTeam(id, false);

        [HttpGet]
        [AllowAnonymous]
        [Route("Ranking")]
        public async Task<Team> Ranking(int teamId) => await _service.GetTeam(teamId, true);

        [HttpPost]
        [Route("ToggleTeamPlayer")]
        public async Task<Team> ToggleTeamPlayer([FromBody]TeamToggleRequest req)
        {
            var result = await _service.ToggleTeamPlayer(req);
            return result;
        }

        [HttpGet]
        [Route("ExcelExport")]
        public async Task<string> GetExcelExport()
        {
            var excel = await _service.GetExcelExport();
            return Convert.ToBase64String(excel);
        }
    }
}
