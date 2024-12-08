using System;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using Ttc.DataAccess.Services;
using Ttc.DataAccess.Utilities;

namespace Ttc.WebApi.Emailing
{
    public class NewPasswordRequestEmailer
    {
        private readonly EmailConfig _config;
        private const string NewPasswordRequestTemplate = @"
Reset je paswoord hier:<br>
<a href='{0}'>{0}</a>
";
        
        public NewPasswordRequestEmailer(EmailConfig emailConfig)
        {
            _config = emailConfig;
        }

        public void Email(string email, Guid guid)
        {
            string subject = "Paswoord reset TTC Aalst";
            string fullUrlLink = $"http://www.ttc-aalst.be/login/nieuw-paswoord/" + guid;
            string content = string.Format(NewPasswordRequestTemplate, fullUrlLink);
            EmailService.SendEmail(email, subject, content, _config).Wait();
        }
    }
}
