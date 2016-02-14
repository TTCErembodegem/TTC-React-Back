using System.Collections.Generic;
using Ttc.DataAccess.Services;
using Ttc.Model.Clubs;
using Ttc.WebApi.Utilities;

namespace Ttc.WebApi.Controllers
{
    public class ClubsController : BaseController
    {
        #region Constructor
        private readonly ClubService _service;

        public ClubsController(ClubService service)
        {
            _service = service;
        }
        #endregion

        public IEnumerable<Club> Get() => _service.GetActiveClubs();
    }
}
