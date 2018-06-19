using System;
using Ttc.DataAccess.Services;
using Ttc.DataAccess.Utilities;

namespace Ttc.WebApi.Emailing
{
    public class PasswordChangedEmailer
    {
        private readonly EmailConfig _config;
        private const string NewPasswordRequestTemplate = @"
Je paswoord is aangepast!<br>
Als je dit niet zelf gedaan hebt, dan is er iets mis!<br>
";

        public PasswordChangedEmailer(EmailConfig emailConfig)
        {
            _config = emailConfig;
        }

        public void Email(string email)
        {
            string subject = "Nieuw paswoord TTC Erembodegem";
            string content = string.Format(NewPasswordRequestTemplate);
            EmailService.SendEmail(email, subject, content, _config).Wait();
        }
    }
}