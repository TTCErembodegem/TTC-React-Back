using System.Collections.Generic;
using Ttc.DataAccess.Services;
using Ttc.Model.Players;
using Ttc.WebApi.Utilities;

namespace Ttc.WebApi.Controllers
{
    public class PlayersController : BaseController
    {
        #region Constructor
        private readonly PlayerService _service;

        public PlayersController(PlayerService service)
        {
            _service = service;
        }
        #endregion

        public IEnumerable<Player> Get() => _service.GetActiveOwnClub();
    }
}
