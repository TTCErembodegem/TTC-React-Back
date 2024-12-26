using FrenoyVttl;
using Ttc.DataEntities.Core;
using Ttc.Model.Players;
using Ttc.Model.Teams;

namespace Frenoy.Api;

public class FrenoyTeamsApi : FrenoyApiBase
{
    public FrenoyTeamsApi(ITtcDbContext ttcDbContext, Competition comp) : base(ttcDbContext, comp)
    {

    }

    public async Task<IList<DivisionRanking>> GetTeamRankings(int divisionId)
    {
        try
        {
            var rankingsResult = await _frenoy.GetDivisionRankingAsync(new GetDivisionRankingRequest1
            {
                GetDivisionRankingRequest = new GetDivisionRankingRequest()
                {
                    DivisionId = divisionId.ToString(),
                }
            });

            var rankings = rankingsResult.GetDivisionRankingResponse.RankingEntries
                .Select(x =>
                {
                    var rank = new DivisionRanking
                    {
                        Position = int.Parse(x.Position),
                        GamesDraw = int.Parse(x.GamesDraw),
                        GamesWon = int.Parse(x.GamesWon),
                        GamesLost = int.Parse(x.GamesLost),
                        Points = int.Parse(x.Points),
                        ClubId = GetClubId(x.TeamClub).Result,
                        TeamCode = ExtractTeamCodeFromFrenoyName(x.Team) ?? "",
                        IsForfait = ExtractIsForfaitFromFrenoyName(x.Team),
                    };
                    return rank;
                }).ToList();

            return rankings;
        }
        catch
        {
            return new List<DivisionRanking>();
        }
    }
}
