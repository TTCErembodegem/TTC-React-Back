using System.Collections.Generic;
using System.Linq;
using Frenoy.Api.FrenoyVttl;
using Ttc.DataEntities.Core;
using Ttc.Model.Players;
using Ttc.Model.Teams;

namespace Frenoy.Api
{
    public class FrenoyTeamsApi : FrenoyApiBase
    {
        public FrenoyTeamsApi(ITtcDbContext ttcDbContext, Competition comp) : base(ttcDbContext, comp)
        {

        }

        public ICollection<DivisionRanking> GetTeamRankings(int divisionId)
        {
            try
            {
                var rankings = _frenoy.GetDivisionRanking(new GetDivisionRankingRequest
                {
                    DivisionId = divisionId.ToString(),
                });

                return rankings.RankingEntries
                    .Select(x => new DivisionRanking
                    {
                        Position = int.Parse(x.Position),
                        GamesDraw = int.Parse(x.GamesDraw),
                        GamesWon = int.Parse(x.GamesWon),
                        GamesLost = int.Parse(x.GamesLost),
                        Points = int.Parse(x.Points),
                        ClubId = GetClubId(x.TeamClub),
                        TeamCode = ExtractTeamCodeFromFrenoyName(x.Team)
                    }).ToArray();
            }
            catch
            {
                return new List<DivisionRanking>();
            }
        }
    }
}