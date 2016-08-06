using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
            var result = _service.GetOwnClub();
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

        [HttpPost]
        [Route("UpdatePlayer")]
        public Player UpdatePlayer([FromBody]Player player)
        {
            var result = _service.UpdatePlayer(player);
            return result;
        }
    
        [HttpGet]
        [Route("ExcelExport")]
        public string GetExcelExport()
        {
            var excel = _service.GetExcelExport();
            return Convert.ToBase64String(excel);
        }
    }
}
