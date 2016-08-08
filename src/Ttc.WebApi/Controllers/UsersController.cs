using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using JWT;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using Ttc.DataAccess.Services;
using Ttc.Model;
using Ttc.Model.Players;
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
        [Route("RequestNewPassword")]
        [AllowAnonymous]
        public void RequestNewPassword([FromBody]NewPasswordRequest request)
        {
            var newPassword = _service.RequestNewPassword(request);
            var emailConfig = _configService.GetEmailConfig();
            CreateNewPasswordRequestEmail(request.Email, newPassword, emailConfig).Wait();
        }

        [HttpPost]
        [Route("SetNewPassword")]
        [AllowAnonymous]
        public void SetNewPassword([FromBody]PasswordCredentials request)
        {
            string playerEmail = _service.SetNewPassword(request);
            var emailConfig = _configService.GetEmailConfig();
            CreateNewPasswordRequestEmail(playerEmail, request.NewPassword, emailConfig).Wait();
        }

        private async Task CreateNewPasswordRequestEmail(string email, string newPassword, EmailConfig config)
        {
            dynamic sg = new SendGridAPIClient(config.SendGridApiKey);
            Email from = new Email(config.EmailFrom);
            Email to = new Email(email);

            string subject = "Nieuw paswoord TTC Erembodegem";
            Content content = new Content("text/plain", "Je nieuw paswoord is: " + newPassword);
            Mail mail = new Mail(from, subject, to, content);

            dynamic response = await sg.client.mail.send.post(requestBody: mail.Get());
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