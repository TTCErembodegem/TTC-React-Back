using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ttc.DataAccess.Utilities;
using Ttc.Model.Teams;
using Ttc.DataEntities;
using Ttc.DataEntities.Core;
using Ttc.Model.Players;

namespace Ttc.DataAccess.Services;

public class TeamService
{
    private readonly ITtcDbContext _context;
    private readonly IMapper _mapper;
    private static readonly TimeSpan FrenoyTeamRankingExpiration = TimeSpan.FromHours(1);

    public TeamService(ITtcDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<Team>> GetForCurrentYear()
    {
        int currentSeason = _context.CurrentSeason;
        var teams = await _context.Teams
            .Include(x => x.Players)
            .Include(x => x.Opponents)
            .Where(x => x.Year == currentSeason)
            .ToArrayAsync();

        var result = _mapper.Map<IList<TeamEntity>, IList<Team>>(teams);
        foreach (var team in result)
        {
            var key = new TeamRankingKey(team.Competition, team.Frenoy.DivisionId);
            if (RankingCache.TryGetValue(key, out ICollection<DivisionRanking>? value))
            {
                team.Ranking = value;
            }
        }

        InvalidateCache();

        return result;
    }

    public async Task<Team> GetTeam(int teamId, bool syncFrenoy)
    {
        var teamEntity = await _context.Teams
            .Include(x => x.Players)
            .Include(x => x.Opponents)
            .SingleAsync(x => x.Id == teamId);

        var team = _mapper.Map<TeamEntity, Team>(teamEntity);
        if (syncFrenoy)
        {
            team.Ranking = await GetFrenoyRanking(team.Competition, team.Frenoy.DivisionId);
        }
        return team;
    }

    private static async Task<ICollection<DivisionRanking>> GetFrenoyRanking(Competition competition, int divisionId)
    {
        var key = new TeamRankingKey(competition, divisionId);
        if (RankingCache.ContainsKey(key))
        {
            return RankingCache[key];
        }

        return null;

        //var frenoy = new FrenoyTeamsApi(dbContext, competition);
        //var ranking = await frenoy.GetTeamRankings(divisionId);
        //if (!RankingCache.ContainsKey(key))
        //{
        //    lock (CacheLock)
        //    {
        //        if (!RankingCache.ContainsKey(key))
        //        {
        //            RankingCache.Add(key, ranking);
        //        }
        //    }
        //}
        //return ranking;
    }

    #region DivisionCache
    private static readonly Dictionary<TeamRankingKey, ICollection<DivisionRanking>> RankingCache = new();
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

        public override bool Equals(object? obj)
        {
            if (obj is not TeamRankingKey otherKey)
                return false;

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

    public async Task<Team> ToggleTeamPlayer(TeamToggleRequest req)
    {
        var team = _context.Teams.Include(x => x.Players).Single(x => x.Id == req.TeamId);
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
            _context.Entry(exPlayer).State = EntityState.Deleted;
        }
        await _context.SaveChangesAsync();
        return await GetTeam(req.TeamId, false);
    }

    public async Task<byte[]> GetExcelExport()
    {
        int currentSeason = _context.CurrentSeason;
        var teams = await _context.Teams
            .Include(x => x.Players)
            //.Include(x => x.Opponents)
            .Where(x => x.Year == currentSeason)
            .ToArrayAsync();

        int currentFrenoySeason = _context.CurrentFrenoySeason;
        var matches = await _context.Matches
            //.Include(x => x.HomeTeam)
            //.Include(x => x.AwayTeam)
            .Include(x => x.Players)
            .Where(x => x.HomeClubId == Constants.OwnClubId || x.AwayClubId == Constants.OwnClubId)
            .Where(x => x.FrenoySeason == currentFrenoySeason)
            .ToListAsync();

        var players = await _context.Players.Where(x => x.Gestopt == null).ToArrayAsync();

        var clubs = await _context.Clubs.ToArrayAsync();

        var exceller = TeamsExcelCreator.CreateFormation(teams, matches, players, clubs);
        return exceller.Create();
    }
}

public class TeamToggleRequest
{
    public int TeamId { get; set; }
    public int PlayerId { get; set; }
    public string Role { get; set; }

    public override string ToString() => $"TeamId={TeamId}, PlayerId={PlayerId}, Role={Role}";
}

