using System;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using Ttc.DataAccess.Services;
using Ttc.DataAccess.Utilities;

namespace Ttc.WebApi.Emailing
{
    public static class EmailService
    {
        public static async Task SendEmail(string email, string subject, string content, EmailConfig config)
        {
            dynamic sg = new SendGridAPIClient(config.SendGridApiKey);
            Email from = new Email(config.EmailFrom);
            Email to = new Email(email);
            Mail mail = new Mail(from, subject, to, new Content("text/html", content));
            dynamic response = await sg.client.mail.send.post(requestBody: mail.Get());
        }
    }
}