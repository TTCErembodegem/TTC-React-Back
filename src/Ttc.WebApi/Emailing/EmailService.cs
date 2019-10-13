using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using SendGrid;
using SendGrid.Helpers.Mail;
using Ttc.DataAccess.Services;
using Ttc.DataAccess.Utilities;
using Ttc.Model.Players;

namespace Ttc.WebApi.Emailing
{
    // SendGrid API Example usage:
    // https://github.com/sendgrid/sendgrid-csharp/blob/master/USE_CASES.md
    public static class EmailService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(EmailService));
        public static async Task SendEmail(IEnumerable<Player> players, string subject, string content, EmailConfig config)
        {
            var client = new SendGridClient(config.SendGridApiKey);
            var from = new EmailAddress(config.EmailFrom);
            var tos = players.Select(player => new EmailAddress(player.Contact.Email, player.FirstName + " " + player.LastName)).ToList();
            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, subject, content, content, true);
            var response = await client.SendEmailAsync(msg);
            await CheckSendGridResponse(subject, response);
        }

        public static async Task SendEmail(string email, string subject, string content, EmailConfig config)
        {
            var client = new SendGridClient(config.SendGridApiKey);
            var from = new EmailAddress(config.EmailFrom);
            var to = new EmailAddress(email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, content, content);
            var response = await client.SendEmailAsync(msg);
            await CheckSendGridResponse(subject, response);
        }

        private static async Task CheckSendGridResponse(string subject, Response response)
        {
            string statusCode = response.StatusCode.ToString();
            if (statusCode != "Accepted")
            {
                string troubles = await response.Body.ReadAsStringAsync();
                Logger.Error($"SendEmail {subject} returned {statusCode}{Environment.NewLine}{troubles}");
                throw new Exception("Error sending email:" + Environment.NewLine + troubles);
            }
        }
    }
}