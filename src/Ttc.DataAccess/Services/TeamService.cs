using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Ttc.DataAccess.Entities;
using Ttc.Model;
using System.Data.Entity;
using Omu.ValueInjecter;
using Ttc.Model.Teams;
using Mapper = AutoMapper.Mapper;

namespace Ttc.DataAccess.Services
{
    public class TeamService
    {
        public IEnumerable<Team> GetForCurrentYear()
        {
            using (var dbContext = new TtcDbContext())
            {
                var activeClubs = dbContext.Reeksen
                    .Include(x => x.Ploegen)
                    .Where(x => x.Jaar == Constants.CurrentSeason)
                    .ToList();

                var result = Mapper.Map<IList<Reeks>, IList<Team>>(activeClubs);
                var otherTeamDivisions = GetMultipleTeamsInDivisions(result);

                // filter own team from opponents
                foreach (var division in result)
                {
                    division.Opponents = division.Opponents.Where(x => x.ClubId != Constants.OwnClubId || x.TeamCode != division.TeamCode).ToArray();
                }

                result = result.Concat(otherTeamDivisions).ToArray();

                // add erembodegem players to team
                foreach (var division in result)
                {
                    var clubTeam = dbContext.ClubPloegen
                        .Include(x => x.Spelers)
                        .First(x => x.ReeksId == division.Id && x.ClubId == Constants.OwnClubId && x.Code == division.TeamCode);

                    division.Players = clubTeam.Spelers.Select(x => new TeamPlayer
                    {
                        PlayerId = x.SpelerId.Value,
                        Type = (TeamPlayerType)x.Kapitein
                    }).ToArray();
                }

                return result;
            }
        }

        /// <summary>
        /// 'Fix' when having multiple Teams in same Reeks/Division
        /// </summary>
        private static IEnumerable<Team> GetMultipleTeamsInDivisions(IEnumerable<Team> result)
        {
            var otherTeamDivisions = new List<Team>(3);
            var multiple = result.Where(x => x.Opponents.Count(team => team.ClubId == Constants.OwnClubId) > 1);
            foreach (var division in multiple)
            {
                var otherOwnOpponents = division.Opponents.Where(x => x.ClubId == Constants.OwnClubId).Skip(1); // original mapping took the first own club team as TeamCode
                foreach (var otherOwnTeam in otherOwnOpponents)
                {
                    var clone = new Team();
                    clone.InjectFrom(division);
                    clone.TeamCode = otherOwnTeam.TeamCode;
                    clone.Opponents = division.Opponents.Where(x => x.ClubId != Constants.OwnClubId || x.TeamCode != otherOwnTeam.TeamCode).ToArray();

                    otherTeamDivisions.Add(clone);
                }
            }
            return otherTeamDivisions;
        }
    }
}
