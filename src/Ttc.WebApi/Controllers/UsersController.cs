using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ttc.DataAccess.Services;
using Ttc.Model;
using Ttc.WebApi.Emailing;
using Ttc.WebApi.Utilities.Auth;

namespace Ttc.WebApi.Controllers;

[Authorize]
[Route("api/users")]
public class UsersController : ControllerBase
{
    #region Constructor
    private readonly PlayerService _service;
    private readonly ConfigService _configService;
    private readonly EmailService _emailService;
    private readonly UserProvider _user;

    public UsersController(
        PlayerService service,
        ConfigService configService,
        EmailService emailService,
        UserProvider user)
    {
        _service = service;
        _configService = configService;
        _emailService = emailService;
        _user = user;
    }
    #endregion

    [HttpPost]
    [Route("Login")]
    [AllowAnonymous]
    public async Task<User?> Login([FromBody] UserCredentials user)
    {
        var player = await _service.Login(user);
        if (player != null)
        {
            player.Token = _user.GenerateJwtToken(player);
        }
        return player;
    }

    [HttpPost]
    [Route("ChangePassword")]
    public async Task<User?> ChangePassword([FromBody] PasswordCredentials userNewPassword)
    {
        var player = await _service.ChangePassword(userNewPassword);
        if (player != null)
        {
            player.Token = _user.GenerateJwtToken(player);
        }
        return player;
    }

    [HttpPost]
    [Route("SetNewPasswordFromGuid")]
    [AllowAnonymous]
    public async Task SetNewPasswordFromGuid([FromBody] NewPasswordRequest request)
    {
        await _service.SetNewPasswordFromGuid(request.Guid, request.PlayerId, request.Password);
    }

    [HttpPost]
    [Route("RequestResetPasswordLink")]
    [AllowAnonymous]
    public async Task RequestResetPasswordLink([FromBody] NewPasswordLinkRequest request)
    {
        Guid resetLinkId = await _service.EmailMatchesPlayer(request.Email, request.PlayerId);

        var emailConfig = await _configService.GetEmailConfig();
        var email = new NewPasswordRequestEmailer(emailConfig, _emailService);
        await email.Email(request.Email, resetLinkId);
    }

    [HttpPost]
    [Route("AdminSetNewPassword")]
    public async Task AdminSetNewPassword([FromBody] PasswordCredentials request)
    {
        string? playerEmail = await _service.SetNewPassword(request);
        if (!string.IsNullOrWhiteSpace(playerEmail))
        {
            var emailConfig = await _configService.GetEmailConfig();
            var email = new PasswordChangedEmailer(emailConfig, _emailService);
            await email.Email(playerEmail);
        }
    }

    [HttpPost]
    [Route("ValidateToken")]
    [AllowAnonymous]
    public async Task<User?> ValidateToken([FromBody] ValidateTokenRequest token)
    {
        var validated = _user.ValidateToken(token.Token);
        if (validated == null)
        {
            return null;
        }

        var userModel = await _service.GetUser(validated.PlayerId);
        userModel.Token = _user.GenerateJwtToken(userModel);
        return userModel;
    }
}
