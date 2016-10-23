using System;
using System.Collections.Generic;
using System.Linq;
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
        [Route("ChangePassword")]
        public User ChangePassword([FromBody]PasswordCredentials userNewPassword)
        {
            var player = _service.ChangePassword(userNewPassword);
            if (player != null)
            {
                player.Token = TtcAuthorizationFilterAttribute.CreateToken(player);
            }
            return player;
        }

        [HttpPost]
        [Route("SetNewPasswordFromGuid")]
        [AllowAnonymous]
        public void SetNewPasswordFromGuid([FromBody]NewPasswordRequest request)
        {
            _service.SetNewPasswordFromGuid(request.Guid, request.PlayerId, request.Password);
        }

        [HttpPost]
        [Route("RequestResetPasswordLink")]
        [AllowAnonymous]
        public void RequestResetPasswordLink([FromBody]NewPasswordLinkRequest request)
        {
            Guid paswoordResetLinkId = _service.EmailMatchesPlayer(request.Email, request.PlayerId);

            var emailConfig = _configService.GetEmailConfig();
            var emailer = new NewPasswordRequestEmailer(emailConfig);
            emailer.Email(request.Email, paswoordResetLinkId);
        }

        [HttpPost]
        [Route("AdminSetNewPassword")]
        public void AdminSetNewPassword([FromBody]PasswordCredentials request)
        {
            string playerEmail = _service.SetNewPassword(request);

            // TODO: Send email when Admin resets someones password?
            //var emailConfig = _configService.GetEmailConfig();
            //var emailer = new PasswordChangedEmailer(emailConfig);
            //emailer.Email(playerEmail);
        }

        [HttpPost]
        [Route("ValidateToken")]
        [AllowAnonymous]
        public User ValidateToken([FromBody]ValidateTokenRequest token)
        {
            var validated = TtcAuthorizationFilterAttribute.ValidateToken(token.Token);
            if (validated == null)
            {
                return null;
            }

            var userModel = _service.GetUser(validated.PlayerId);
            userModel.Token = TtcAuthorizationFilterAttribute.CreateToken(userModel);
            return userModel;
        }
    }
}