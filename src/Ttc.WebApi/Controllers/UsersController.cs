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
        [Route("Login")]
        public User Login([FromBody]UserCredentials user)
        {
            var player = _service.Login(user);
            if (player != null)
            {
                player.Token = CreateToken(player);
            }
            return player;
        }

        [HttpPost]
        [Route("ValidateToken")]
        public User ValidateToken([FromBody]ValidateTokenRequest token)
        {
            string apikey = WebApi.Properties.Settings.Default.JwtSecret;
            dynamic decodedToken = JsonConvert.DeserializeObject(JsonWebToken.Decode(token.Token, apikey));
            int userId = (int)decodedToken.userId;
            User user = _service.GetUser(userId);
            user.Token = token.Token;
            return user;
        }

        private static string CreateToken(User user)
        {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var expiry = Math.Round((DateTime.UtcNow.AddMonths(6) - unixEpoch).TotalSeconds);
            var issuedAt = Math.Round((DateTime.UtcNow - unixEpoch).TotalSeconds);
            var notBefore = Math.Round((DateTime.UtcNow.AddDays(-1) - unixEpoch).TotalSeconds);

            var payload = new Dictionary<string, object>
            {
                {"alias", user.Alias},
                {"userId", user.PlayerId},
                {"role", "Admin"  },
                {"sub", user.PlayerId},
                {"nbf", notBefore},
                {"iat", issuedAt},
                {"exp", expiry}
            };

            string apikey = WebApi.Properties.Settings.Default.JwtSecret;
            var token = JsonWebToken.Encode(payload, apikey, JwtHashAlgorithm.HS256);
            return token;
        }
    }

    public class ValidateTokenRequest
    {
        public string Token { get; set; }
    }
}