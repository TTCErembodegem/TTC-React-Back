using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Ttc.DataAccess;
using Ttc.DataAccess.Services;
using Ttc.Model;

namespace Ttc.WebApi.Controllers
{
    public class PlayersController : ApiController
    {
        #region Constructor
        private readonly PlayerService _playerService;

        public PlayersController(PlayerService playerService)
        {
            _playerService = playerService;
        }
        #endregion

        public IEnumerable<Player> Get()
        {
            return _playerService.GetActiveOwnClub();
        }
    }
}
