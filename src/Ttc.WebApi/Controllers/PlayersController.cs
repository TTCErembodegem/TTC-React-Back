using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<Player> Get()
        {
            // TODO: we zaten hier: Token valideren en gevoelige data verwijderen...
            //var headerValues = Request.Headers.Authorization.Parameter;

            var result = _service.GetActiveOwnClub();
            return result;
        } 
    }
}
