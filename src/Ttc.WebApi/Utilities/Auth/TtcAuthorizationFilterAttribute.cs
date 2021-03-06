﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using JWT;
using Newtonsoft.Json;
using Ttc.DataAccess.Services;
using Ttc.Model;

namespace Ttc.WebApi.Utilities.Auth
{
    public class TtcAuthorizationFilterAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (actionContext.Request.Headers.Authorization == null)
            {
                return false;
            }

            var token = actionContext.Request.Headers.Authorization.Parameter;
            var user = ValidateToken(token);
            if (user != null)
            {
                actionContext.RequestContext.Principal = new TtcPrincipal(user);
                return true;
            }

            return false;
        }

        public static string CreateToken(User user)
        {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var expiry = Math.Round((DateTime.UtcNow.AddMonths(6) - unixEpoch).TotalSeconds);
            var issuedAt = Math.Round((DateTime.UtcNow - unixEpoch).TotalSeconds);
            var notBefore = Math.Round((DateTime.UtcNow.AddDays(-1) - unixEpoch).TotalSeconds);

            var payload = new Dictionary<string, object>
            {
                {"alias", user.Alias},
                {"playerId", user.PlayerId},
                {"security", JsonConvert.SerializeObject(user.Security) },
                {"teams", string.Join(",", user.Teams) },
                {"sub", user.PlayerId},
                {"nbf", notBefore},
                {"iat", issuedAt},
                {"exp", expiry}
            };

            string apikey = WebApi.Properties.Settings.Default.JwtSecret;
            var token = JsonWebToken.Encode(payload, apikey, JwtHashAlgorithm.HS256);
            return token;
        }

        public static bool IsValidToken(string token)
        {
            try
            {
                string apikey = WebApi.Properties.Settings.Default.JwtSecret;
                JsonConvert.DeserializeObject(JsonWebToken.Decode(token, apikey));
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static User ValidateToken(string token)
        {
            dynamic user;
            try
            {
                string apikey = WebApi.Properties.Settings.Default.JwtSecret;
                user = JsonConvert.DeserializeObject(JsonWebToken.Decode(token, apikey));
            }
            catch
            {
                return null;
            }

            var userModel = new User
            {
                Alias = (string)user.alias,
                PlayerId = (int)user.playerId,
                Teams = !string.IsNullOrWhiteSpace((string)user.teams) ? ((string)user.teams).Split(',').Select(int.Parse).ToArray() : new int[] { },
            };

            userModel.Security = JsonConvert.DeserializeObject<List<string>>((string)user.security);
            userModel.Token = token;
            return userModel;
        }
    }
}