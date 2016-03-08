using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using JWT;
using Ttc.DataAccess.Services;
using Ttc.Model;
using Ttc.WebApi.Utilities;

namespace Ttc.WebApi.Controllers
{
    [AllowAnonymous]
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
        //[Route("Login")]
        //[ValidateAntiForgeryToken]
        public User Login([FromBody]UserCredentials user)
        {
            var player = _service.Login(user);
            if (player != null)
            {
                var token = CreateToken(player);
            }
            return player;
        }

        private static string CreateToken(User user)
        {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var expiry = Math.Round((DateTime.UtcNow.AddHours(2) - unixEpoch).TotalSeconds);
            var issuedAt = Math.Round((DateTime.UtcNow - unixEpoch).TotalSeconds);
            var notBefore = Math.Round((DateTime.UtcNow.AddMonths(6) - unixEpoch).TotalSeconds);

            var payload = new Dictionary<string, object>
            {
                //{"email", user.Email},
                {"userId", user.PlayerId},
                {"role", "Admin"  },
                {"sub", user.PlayerId},
                {"nbf", notBefore},
                {"iat", issuedAt},
                {"exp", expiry}
            };

            //var secret = ConfigurationManager.AppSettings.Get("jwtKey");
            const string apikey = "secretKey";

            var token = JsonWebToken.Encode(payload, apikey, JwtHashAlgorithm.HS256);
            return token;
        }
    }
}