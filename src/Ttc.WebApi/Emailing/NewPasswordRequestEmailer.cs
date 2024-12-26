using Ttc.DataAccess.Utilities;

namespace Ttc.WebApi.Emailing;

public class NewPasswordRequestEmailer
{
    private readonly EmailConfig _config;
    private readonly EmailService _emailService;

    private const string NewPasswordRequestTemplate = @"
Reset je paswoord hier:<br>
<a href='{0}'>{0}</a>
";

    public NewPasswordRequestEmailer(EmailConfig emailConfig, EmailService emailService)
    {
        _config = emailConfig;
        _emailService = emailService;
    }

    public async Task Email(string email, Guid guid)
    {
        string subject = "Paswoord reset TTC Aalst";
        string fullUrlLink = "http://www.ttc-aalst.be/login/nieuw-paswoord/" + guid;
        string content = string.Format(NewPasswordRequestTemplate, fullUrlLink);
        await _emailService.SendEmail(email, subject, content, _config);
    }
}
