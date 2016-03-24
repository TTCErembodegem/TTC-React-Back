using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Ttc.DataAccess.Services;
using Ttc.Model.Players;
using Ttc.WebApi.Utilities;
using Ttc.WebApi.Utilities.Auth;

namespace Ttc.WebApi.Controllers
{
    [RoutePrefix("api/players")]
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
            CleanSensitiveData(result);
            return result;
        }

        [AllowAnonymous]
        public Player Get(int id)
        {
            var result = _service.GetPlayer(id);
            CleanSensitiveData(result);
            return result;
        }

        [HttpPost]
        [Route("UpdateStyle")]
        public Player UpdateStyle([FromBody]PlayerStyle playerStyle)
        {
            var result = _service.UpdateStyle(playerStyle);
            return result;
        }
    }
}
