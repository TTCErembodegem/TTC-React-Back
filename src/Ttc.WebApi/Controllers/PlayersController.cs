using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Ttc.DataAccess;
using Ttc.DataAccess.Services;
using Ttc.Model;
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
