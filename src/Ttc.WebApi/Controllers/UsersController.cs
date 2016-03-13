using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using JWT;
using Newtonsoft.Json;
using Ttc.DataAccess.Services;
using Ttc.Model;
using Ttc.WebApi.Utilities;
using Ttc.WebApi.Utilities.Auth;

namespace Ttc.WebApi.Controllers
{
    [RoutePrefix("api/users")]
    public class UsersController : BaseController
    {
        #region Constructor
        private readonly PlayerService _service;

        public UsersController(PlayerService service)
        {
            _service = service;
        }
        #endregion

        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public User Login([FromBody]UserCredentials user)
        {
            var player = _service.Login(user);
            if (player != null)
            {
                player.Token = TtcAuthorizationFilterAttribute.CreateToken(player);
            }
            return player;
        }

        [HttpPost]
        [Route("ValidateToken")]
        [AllowAnonymous]
        public User ValidateToken([FromBody]ValidateTokenRequest token)
        {
            return TtcAuthorizationFilterAttribute.ValidateToken(token.Token);
        }
    }
}