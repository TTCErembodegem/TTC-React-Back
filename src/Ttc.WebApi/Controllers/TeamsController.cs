using System.Collections.Generic;
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
        public IEnumerable<Team> Get() => _service.GetForCurrentYear();

        [AllowAnonymous]
        public Team Get(int id) => _service.GetTeam(id, false);

        [HttpGet]
        [AllowAnonymous]
        [Route("Ranking")]
        public Team Ranking(int teamId) => _service.GetTeam(teamId, true);
    }
}
