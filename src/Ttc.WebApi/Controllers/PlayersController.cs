using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
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
        public async Task<IEnumerable<Player>> Get()
        {
            var result = await _service.GetOwnClub();
            CleanSensitiveData(result);
            return result;
        }

        [AllowAnonymous]
        public async Task<Player> Get(int id)
        {
            var result = await _service.GetPlayer(id, true);
            CleanSensitiveData(result);
            return result;
        }

        [HttpPost]
        [Route("UpdateStyle")]
        public async Task<Player> UpdateStyle([FromBody]PlayerStyle playerStyle)
        {
            var result = await _service.UpdateStyle(playerStyle);
            return result;
        }

        [HttpPost]
        [Route("UpdatePlayer")]
        public async Task<Player> UpdatePlayer([FromBody]Player player)
        {
            var result = await _service.UpdatePlayer(player);
            return result;
        }

        [HttpPost]
        [Route("DeletePlayer/{playerId}")]
        public async Task DeletePlayer(int playerId)
        {
            await _service.DeletePlayer(playerId);
        }

        [HttpPost]
        [Route("FrenoySync")]
        public async Task FrenoySync()
        {
            await _service.FrenoySync();
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
