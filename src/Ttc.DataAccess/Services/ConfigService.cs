using Microsoft.EntityFrameworkCore;
using Ttc.DataAccess.Utilities;
using Ttc.DataEntities.Core;

namespace Ttc.DataAccess.Services;

public class ConfigService
{
    private readonly ITtcDbContext _context;

    public ConfigService(ITtcDbContext context)
    {
        _context = context;
    }

    public async Task<Dictionary<string, string>> Get()
    {
        var keys = new[]
        {
            "email", "googleMapsUrl", "location", "trainingDays", "competitionDays",
            "adultMembership", "youthMembership", "additionalMembership", "recreationalMembers",
            "frenoyClubIdVttl", "frenoyClubIdSporta", "compBalls", "clubBankNr", "clubOrgNr", "year",
            "endOfSeason"
        };

        var parameters = await _context.Parameters.Where(x => keys.Contains(x.Sleutel)).ToArrayAsync();
        return parameters.ToDictionary(x => x.Sleutel, x => x.Value);
    }

    public async Task<EmailConfig> GetEmailConfig()
    {
        var sendGridApiKey = (await _context.Parameters.SingleAsync(x => x.Sleutel == "SendGridApiKey")).Value;
        var fromEmail = (await _context.Parameters.SingleAsync(x => x.Sleutel == "FromEmail")).Value;
        return new EmailConfig(fromEmail, sendGridApiKey);
    }

    public async Task Save(string key, string value)
    {
        var param = await _context.Parameters.SingleAsync(x => x.Sleutel == key);
        if (key == "year")
        {
            int newYear = await NewSeasonSeed.Seed(_context, false);
            param.Value = newYear.ToString();
        }
        else
        {
            param.Value = value;
        }
        await _context.SaveChangesAsync();
    }
}