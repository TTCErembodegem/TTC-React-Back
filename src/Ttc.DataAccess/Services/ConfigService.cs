using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Ttc.DataAccess.Utilities;

namespace Ttc.DataAccess.Services
{
    public class ConfigService : BaseService
    {
        public async Task<Dictionary<string, string>> Get()
        {
            var dict = new Dictionary<string, string>();
            using (var context = new TtcDbContext())
            {
                var keys = new[]
                {
                    "email", "googleMapsUrl", "location", "trainingDays", "competitionDays",
                    "adultMembership", "youthMembership", "additionalMembership", "recreationalMembers",
                    "frenoyClubIdVttl", "frenoyClubIdSporta", "compBalls", "clubBankNr", "clubOrgNr", "year",
                    "endOfSeason"
                };
                foreach (var parameter in (await context.Parameters.ToArrayAsync()).Where(x => keys.Contains(x.Sleutel)))
                {
                    dict.Add(parameter.Sleutel, parameter.Value);
                }
            }
            return dict;
        }

        public async Task<EmailConfig> GetEmailConfig()
        {
            using (var context = new TtcDbContext())
            {
                var sendGridApiKey = (await context.Parameters.SingleAsync(x => x.Sleutel == "SendGridApiKey")).Value;
                var fromEmail = (await context.Parameters.SingleAsync(x => x.Sleutel == "FromEmail")).Value;
                return new EmailConfig(fromEmail, sendGridApiKey);
            }
        }

        public async Task Save(string key, string value)
        {
            using (var context = new TtcDbContext())
            {
                var param = await context.Parameters.SingleAsync(x => x.Sleutel == key);
                if (key == "year")
                {
                    int newYear = await NewSeasonSeed.Seed(context, false);
                    param.Value = newYear.ToString();
                }
                else
                {
                    param.Value = value;
                }
                await context.SaveChangesAsync();
            }
        }
    }
}