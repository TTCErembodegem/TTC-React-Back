using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using JWT;
using Newtonsoft.Json;
using Ttc.DataAccess.Services;
using Ttc.Model;
using Ttc.Model.Players;
using Ttc.WebApi.Emailing;
using Ttc.WebApi.Utilities;
using Ttc.WebApi.Utilities.Auth;

namespace Ttc.WebApi.Controllers
{
    [RoutePrefix("api/users")]
    public class UsersController : BaseController
    {
        #region Constructor
        private readonly PlayerService _service;
        private readonly ConfigService _configService;

        public UsersController(PlayerService service, ConfigService configService)
        {
            _service = service;
            _configService = configService;
        }
        #endregion

        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public async Task<User> Login([FromBody]UserCredentials user)
        {
            var player = await _service.Login(user);
            if (player != null)
            {
                player.Token = TtcAuthorizationFilterAttribute.CreateToken(player);
            }
            return player;
        }

        [HttpPost]
        [Route("ChangePassword")]
        public async Task<User> ChangePassword([FromBody]PasswordCredentials userNewPassword)
        {
            var player = await _service.ChangePassword(userNewPassword);
            if (player != null)
            {
                player.Token = TtcAuthorizationFilterAttribute.CreateToken(player);
            }
            return player;
        }

        [HttpPost]
        [Route("SetNewPasswordFromGuid")]
        [AllowAnonymous]
        public async Task SetNewPasswordFromGuid([FromBody]NewPasswordRequest request)
        {
            await _service.SetNewPasswordFromGuid(request.Guid, request.PlayerId, request.Password);
        }

        [HttpPost]
        [Route("RequestResetPasswordLink")]
        [AllowAnonymous]
        public async Task RequestResetPasswordLink([FromBody]NewPasswordLinkRequest request)
        {
            Guid paswoordResetLinkId = await _service.EmailMatchesPlayer(request.Email, request.PlayerId);

            var emailConfig = await _configService.GetEmailConfig();
            var emailer = new NewPasswordRequestEmailer(emailConfig);
            emailer.Email(request.Email, paswoordResetLinkId);
        }

        [HttpPost]
        [Route("AdminSetNewPassword")]
        public async Task AdminSetNewPassword([FromBody]PasswordCredentials request)
        {
            string playerEmail = await _service.SetNewPassword(request);
            if (!string.IsNullOrWhiteSpace(playerEmail))
            {
                var emailConfig = await _configService.GetEmailConfig();
                var emailer = new PasswordChangedEmailer(emailConfig);
                emailer.Email(playerEmail);
            }
        }

        [HttpPost]
        [Route("ValidateToken")]
        [AllowAnonymous]
        public async Task<User> ValidateToken([FromBody]ValidateTokenRequest token)
        {
            var validated = TtcAuthorizationFilterAttribute.ValidateToken(token.Token);
            if (validated == null)
            {
                return null;
            }

            var userModel = await _service.GetUser(validated.PlayerId);
            userModel.Token = TtcAuthorizationFilterAttribute.CreateToken(userModel);
            return userModel;
        }
    }
}