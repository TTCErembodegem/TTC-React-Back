using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
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

        [AllowAnonymous]
        public IEnumerable<Player> Get()
        {
            var result = _service.GetActiveOwnClub();
            return result;
        } 
    }
}
