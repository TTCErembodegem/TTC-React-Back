using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Frenoy.Api;
using Omu.ValueInjecter;
using Ttc.Model.Teams;
using Mapper = AutoMapper.Mapper;
using Ttc.DataEntities;
using Ttc.Model.Players;

namespace Ttc.DataAccess.Services
{
    public class TeamService
    {
        public IEnumerable<Team> GetForCurrentYear()
        {
            using (var dbContext = new TtcDbContext())
            {
                var teams = dbContext.Teams
                    .Include(x => x.Players)
                    .Include(x => x.Opponents)
                    .Where(x => x.Year == Constants.CurrentSeason)
                    .ToList();

                var result = Mapper.Map<IList<TeamEntity>, IList<Team>>(teams);
                foreach (var team in result)
                {
                    var frenoy = new FrenoyApi(dbContext, team.Competition);
                    var rankings = frenoy.GetTeamRankings(team.Frenoy.DivisionId);

                    team.Ranking = rankings;
                }

                return result;
            }
        }

        // GetTeam heeft dezelfde includes etc nodig als GetForCurrentYear
        //public Team GetTeam(int teamId)
        //{
        //    using (var dbContext = new TtcDbContext())
        //    {
        //        return Mapper.Map<Reeks, Team>(dbContext.Reeksen.SingleOrDefault(x => x.Id == teamId));
        //    }
        //}
    }
}
