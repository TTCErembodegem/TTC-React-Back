using Ttc.DataAccess.Utilities;

namespace Ttc.WebApi.Emailing;

public class PasswordChangedEmailer
{
    private readonly EmailConfig _config;
    private readonly EmailService _emailService;

    private const string NewPasswordRequestTemplate = @"
Je paswoord is aangepast!<br>
Als je dit niet zelf gedaan hebt, dan is er iets mis!<br>
";

    public PasswordChangedEmailer(EmailConfig emailConfig, EmailService emailService)
    {
        _config = emailConfig;
        _emailService = emailService;
    }

    public async Task Email(string email)
    {
        string subject = "Nieuw paswoord TTC Aalst";
        string content = string.Format(NewPasswordRequestTemplate);
        await _emailService.SendEmail(email, subject, content, _config);
    }
}
