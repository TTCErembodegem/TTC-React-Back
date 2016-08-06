using System.Linq;

namespace Ttc.DataAccess.Services
{
    public class EmailConfig
    {
        public string EmailFrom { get; set; }
        public string SendGridApiKey { get; set; }

        public EmailConfig(string emailFrom, string sendGridApiKey)
        {
            EmailFrom = emailFrom;
            SendGridApiKey = sendGridApiKey;
        }

        public override string ToString() => $"{EmailFrom}, ${SendGridApiKey}";
    }

    public class ConfigService : BaseService
    {
        public EmailConfig GetEmailConfig()
        {
            using (var context = new TtcDbContext())
            {
                var sendGridApiKey = context.Parameters.Single(x => x.Sleutel == "SendGridApiKey").Value;
                var fromEmail = context.Parameters.Single(x => x.Sleutel == "FromEmail").Value;
                return new EmailConfig(fromEmail, sendGridApiKey);
            }
        }
    }
}