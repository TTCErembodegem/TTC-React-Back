using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ttc.DataAccess.Utilities;

namespace Ttc.DataAccess.Services
{
    public class ConfigService : BaseService
    {
        public Dictionary<string, string> Get()
        {
            var dict = new Dictionary<string, string>();
            using (var context = new TtcDbContext())
            {
                var keys = new[]
                {
                    "email", "googleMapsUrl", "location", "trainingDays", "competitionDays",
                    "adultMembership", "youthMembership", "additionalMembership", "recreationalMembers",
                    "frenoyClubIdVttl", "frenoyClubIdSporta"
                };
                foreach (var parameter in context.Parameters.ToArray().Where(x => keys.Contains(x.Sleutel)))
                {
                    dict.Add(parameter.Sleutel, parameter.Value);
                }
            }
            return dict;
        }

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