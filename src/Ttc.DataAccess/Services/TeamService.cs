using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Frenoy.Api;
using Frenoy.Api.FrenoySporta;
using OfficeOpenXml;
using Omu.ValueInjecter;
using Ttc.DataAccess.Utilities;
using Ttc.Model.Teams;
using Mapper = AutoMapper.Mapper;
using Ttc.DataEntities;
using Ttc.Model.Players;

namespace Ttc.DataAccess.Services
{
    public class TeamService : BaseService
    {
        private static readonly TimeSpan FrenoyTeamRankingExpiration = TimeSpan.FromHours(1);

        public IEnumerable<Team> GetForCurrentYear()
        {
            using (var dbContext = new TtcDbContext())
            {
                var teams = dbContext.Teams
                    .Include(x => x.Players)
                    .Include(x => x.Opponents)
                    .Where(x => x.Year == Constants.CurrentSeason)
                    .ToArray();

                var result = Mapper.Map<IList<TeamEntity>, IList<Team>>(teams);
                foreach (var team in result)
                {
                    var key = new TeamRankingKey(team.Competition, team.Frenoy.DivisionId);
                    if (RankingCache.ContainsKey(key))
                    {
                        team.Ranking = RankingCache[key];
                    }
                }

                InvalidateCache();

                return result;
            }
        }

        public Team GetTeam(int teamId, bool syncFrenoy)
        {
            using (var dbContext = new TtcDbContext())
            {
                var teamEntity = dbContext.Teams
                    .Include(x => x.Players)
                    .Include(x => x.Opponents)
                    .Single(x => x.Id == teamId);

                var team = Mapper.Map<TeamEntity, Team>(teamEntity);
                if (syncFrenoy)
                {
                    team.Ranking = GetFrenoyRanking(dbContext, team.Competition, team.Frenoy.DivisionId);
                }
                return team;
            }
        }

        private static ICollection<DivisionRanking> GetFrenoyRanking(TtcDbContext dbContext, Competition competition, int divisionId)
        {
            var key = new TeamRankingKey(competition, divisionId);
            if (RankingCache.ContainsKey(key))
            {
                return RankingCache[key];
            }

            var frenoy = new FrenoyTeamsApi(dbContext, competition);
            var ranking = frenoy.GetTeamRankings(divisionId);
            if (!RankingCache.ContainsKey(key))
            {
                lock (CacheLock)
                {
                    if (!RankingCache.ContainsKey(key))
                    {
                        RankingCache.Add(key, ranking);
                    }
                }
            }
            return ranking;
        }

        #region DivisionCache
        private static readonly Dictionary<TeamRankingKey, ICollection<DivisionRanking>> RankingCache = new Dictionary<TeamRankingKey, ICollection<DivisionRanking>>();
        private static readonly object CacheLock = new object();

        private struct TeamRankingKey
        {
            private readonly DateTime _created;
            private readonly Competition _competition;
            private readonly int _divisionId;

            public bool IsExpired()
            {
                return _created.Add(FrenoyTeamRankingExpiration) < DateTime.Now;
            }

            public TeamRankingKey(Competition competition, int divisionId)
            {
                _competition = competition;
                _divisionId = divisionId;
                _created = DateTime.Now;
            }

            public override string ToString() => $"Competition: {_competition}, DivisionId: {_divisionId}";

            public override bool Equals(object obj)
            {
                if (!(obj is TeamRankingKey))
                    return false;

                var otherKey = (TeamRankingKey)obj;
                return _competition == otherKey._competition && _divisionId == otherKey._divisionId;
            }

            public override int GetHashCode()
            {
                return (_competition + _divisionId.ToString()).GetHashCode();
            }
        }

        private static void InvalidateCache()
        {
            foreach (TeamRankingKey rankingKey in RankingCache.Keys.ToArray())
            {
                if (rankingKey.IsExpired())
                {
                    RankingCache.Remove(rankingKey);
                }
            }
        }
        #endregion

        public Team ToggleTeamPlayer(TeamToggleRequest req)
        {
            using (var db = new TtcDbContext())
            {
                var team = db.Teams.Include(x => x.Players).Single(x => x.Id == req.TeamId);
                var exPlayer = team.Players.SingleOrDefault(x => x.PlayerId == req.PlayerId);
                if (exPlayer == null)
                {
                    team.Players.Add(new TeamPlayerEntity
                    {
                        PlayerId = req.PlayerId,
                        TeamId = req.TeamId,
                        PlayerType = (TeamPlayerType)Enum.Parse(typeof(TeamPlayerType), req.Role)
                    });
                }
                else
                {
                    db.Entry(exPlayer).State = EntityState.Deleted;
                }
                db.SaveChanges();
                return GetTeam(req.TeamId, false);
            }
        }

        public byte[] GetExcelExport()
        {
            using (var dbContext = new TtcDbContext())
            {
                var teams = dbContext.Teams
                    .Include(x => x.Players)
                    //.Include(x => x.Opponents)
                    .Where(x => x.Year == Constants.CurrentSeason)
                    .ToArray();

                var matches = dbContext.Matches
                    .Include(x => x.HomeTeam)
                    .Include(x => x.AwayTeam)
                    .Include(x => x.Players)
                    .Where(x => x.HomeClubId == Constants.OwnClubId || x.AwayClubId == Constants.OwnClubId)
                    .Where(x => x.FrenoySeason == Constants.FrenoySeason)
                    .ToList();

                var players = dbContext.Players.Where(x => x.Gestopt == null).ToArray();

                var clubs = dbContext.Clubs.ToArray();

                var exceller = TeamsExcelCreator.CreateFormation(teams, matches, players, clubs);
                return exceller.Create();
            }
        }
    }

    public class TeamToggleRequest
    {
        public int TeamId { get; set; }
        public int PlayerId { get; set; }
        public string Role { get; set; }

        public override string ToString() => $"TeamId={TeamId}, PlayerId={PlayerId}, Role={Role}";
    }
}
