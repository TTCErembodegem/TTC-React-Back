using System.Collections.Generic;
using Ttc.DataAccess.Services;
using Ttc.Model.Teams;
using Ttc.WebApi.Utilities;

namespace Ttc.WebApi.Controllers
{
    public class TeamsController : BaseController
    {
        #region Constructor
        private readonly TeamService _service;

        public TeamsController(TeamService service)
        {
            _service = service;
        }
        #endregion

        public IEnumerable<Team> Get() => _service.GetForCurrentYear();
    }
}
