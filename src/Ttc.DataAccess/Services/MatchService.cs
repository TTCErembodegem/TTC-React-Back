using System.Diagnostics;
using AutoMapper;
using Frenoy.Api;
using Microsoft.EntityFrameworkCore;
using Ttc.DataAccess.Utilities;
using Ttc.DataEntities;
using Ttc.DataEntities.Core;
using Ttc.Model.Matches;
using Ttc.Model.Players;
using Ttc.Model.Teams;

namespace Ttc.DataAccess.Services;

public class MatchService
{
    private readonly ITtcDbContext _context;
    private readonly IMapper _mapper;

    public MatchService(ITtcDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    #region Getters
    public async Task<IList<Match>> GetMatches()
    {
        var currentFrenoySeason = _context.CurrentFrenoySeason;
        var matchEntities = await _context.Matches
            .WithIncludes()
            //.Where(x => x.Id == 802)
            .Where(x => x.HomeClubId == Constants.OwnClubId || x.AwayClubId == Constants.OwnClubId)
            .Where(x => x.FrenoySeason == currentFrenoySeason)
            .ToListAsync();

        var matchIds = matchEntities.Select(x => x.Id).ToArray();
        var comments = await _context.MatchComments.Where(x => matchIds.Contains(x.MatchId)).ToArrayAsync();
        foreach (var match in matchEntities)
        {
            match.Comments = comments.Where(x => x.MatchId == match.Id).ToArray();
        }

        var result = _mapper.Map<IList<MatchEntity>, IList<Match>>(matchEntities);
        return result;
    }

    public async Task<ICollection<OtherMatch>> GetOpponentMatches(int teamId, OpposingTeam? opponent = null)
    {
        var team = await _context.Teams.SingleAsync(x => x.Id == teamId);

        async Task<MatchEntity[]> GetMatchEntities()
        {
            var matches = _context.Matches
                .WithIncludes()
                .Where(match => match.FrenoyDivisionId == team.FrenoyDivisionId);

            if (opponent != null)
            {
                matches = matches.Where(match =>
                    (match.AwayClubId == opponent.ClubId && match.AwayTeamCode == opponent.TeamCode) ||
                    (match.HomeClubId == opponent.ClubId && match.HomeTeamCode == opponent.TeamCode)
                );
            }

            return await matches.ToArrayAsync();
        }


        MatchEntity[] matchEntities = await GetMatchEntities();

        if (opponent != null)
        {
            // TODO BUG: This means that when called from Team Week Overview, no opponent is set and there is no sync...
            // TODO PERFORMANCE: This executes too many times, make it part of initial competition load
            var frenoy = new FrenoyMatchesApi(_context, Constants.NormalizeCompetition(team.Competition));
            await frenoy.SyncOpponentMatches(team, opponent);

            matchEntities = await GetMatchEntities();
        }

        // No comments for OpponentMatches

        var result = _mapper.Map<IList<MatchEntity>, IList<OtherMatch>>(matchEntities);
        return result;

        // TODO: Bug PreSeason code: This doesn't work! These results are NOT displayed in the MatchCard, the spinner just keeps on spinnin'
        // Pre season: Fetch last year matches instead
        //int? divisionId = frenoy.SyncLastYearOpponentMatches(team, opponent);
        //if (divisionId.HasValue)
        //{
        //    var matchEntities = dbContext.Matches
        //        .WithIncludes()
        //        .Where(match => (match.AwayClubId == opponent.ClubId && match.AwayTeamCode == opponent.TeamCode) || (match.HomeClubId == opponent.ClubId && match.HomeTeamCode == opponent.TeamCode))
        //        .Where(match => match.FrenoyDivisionId == divisionId.Value)
        //        .ToList();

        //    // No comments for OpponentMatches

        //    // HACK: hack om vorig jaar matchen te tonen in de frontend zonder te moeten berekenen wat hun "last year division id" is
        //    foreach (var match in matchEntities)
        //    {
        //        match.FrenoyDivisionId = team.FrenoyDivisionId;
        //    }

        //    var result = Mapper.Map<IList<MatchEntity>, IList<OtherMatch>>(matchEntities);
        //    return result;
        //}
    }
    #endregion

    #region Boilerplate code
    public async Task<Match> GetMatch(int matchId)
    {
        var match = await _context.Matches
                .WithIncludes()
                .SingleAsync(x => x.Id == matchId);

        // TODO: UserProver.IsAuthenticated!
        // TODO: Switch to Postgres and maybe we can just include?
        var comments = await _context.MatchComments.Where(x => x.MatchId == matchId).ToArrayAsync();
        match.Comments = comments;

        var matchModel = _mapper.Map<MatchEntity, Match>(match);
        return matchModel;
    }
    #endregion

    #region Putters
    public async Task<Match> UpdateScore(int matchId, MatchScore score)
    {
        var match = await _context.Matches
            .WithIncludes()
            .SingleAsync(x => x.Id == matchId);

        if (match.IsSyncedWithFrenoy)
        {
            return await GetMatch(match.Id);
        }

        match.AwayScore = score.Out;
        match.HomeScore = score.Home;
        await _context.SaveChangesAsync();

        return await GetMatch(match.Id);
    }

    #region Players
    public async Task<Match> SetMyFormation(MatchPlayer matchPlayer)
    {
        var existingPlayer = await _context.MatchPlayers
            .Include(x => x.Match)
            .Where(x => x.MatchId == matchPlayer.MatchId && x.PlayerId == matchPlayer.PlayerId)
            .FirstOrDefaultAsync(x => x.Status != PlayerMatchStatus.Captain && x.Status != PlayerMatchStatus.Major);

        if (existingPlayer != null)
        {
            existingPlayer.Status = matchPlayer.Status;
            existingPlayer.StatusNote = matchPlayer.StatusNote;
        }
        else
        {
            var verslagSpeler = _mapper.Map<MatchPlayer, MatchPlayerEntity>(matchPlayer);
            _context.MatchPlayers.Add(verslagSpeler);
        }
        await _context.SaveChangesAsync();
        var newMatch = await GetMatch(matchPlayer.MatchId);
        return newMatch;
    }

    /// <summary>
    /// Toggle Captain/Major player by any player of the team on the day of the match
    /// </summary>
    public async Task<Match> ToggleMatchPlayer(MatchPlayer matchPlayer)
    {
        Debug.Assert(matchPlayer.Status is PlayerMatchStatus.Captain or PlayerMatchStatus.Major);
        var match = await _context.Matches.FindAsync(matchPlayer.MatchId);
        if (match == null || match.IsSyncedWithFrenoy)
        {
            return await GetMatch(matchPlayer.MatchId);
        }

        var existingPlayer = await _context.MatchPlayers
            .Where(x => x.MatchId == matchPlayer.MatchId && x.PlayerId == matchPlayer.PlayerId)
            .FirstOrDefaultAsync(x => x.Status == matchPlayer.Status);

        match.Block = matchPlayer.Status;
        if (existingPlayer != null)
        {
            _context.MatchPlayers.Remove(existingPlayer);
        }
        else
        {
            _context.MatchPlayers.Add(_mapper.Map<MatchPlayer, MatchPlayerEntity>(matchPlayer));
        }
        await _context.SaveChangesAsync();
        var newMatch = await GetMatch(matchPlayer.MatchId);
        return newMatch;
    }

    /// <summary>
    /// Set all players for the match to Captain/Major
    /// </summary>
    /// <param name="blockAlso">Also block the match to the newStatus level</param>
    public async Task<Match> EditMatchPlayers(int matchId, int[] playerIds, string newStatus, bool blockAlso, string comment)
    {
        Debug.Assert(newStatus is PlayerMatchStatus.Captain or PlayerMatchStatus.Major);
        var match = await _context.Matches.SingleAsync(x => x.Id == matchId);
        if (match.IsSyncedWithFrenoy)
        {
            return await GetMatch(matchId);
        }

        match.FormationComment = comment;
        match.Block = blockAlso ? newStatus : null;
        var existingPlayers = await _context.MatchPlayers
            .Where(x => x.MatchId == matchId)
            .Where(x => x.Status == newStatus)
            .ToArrayAsync();
        _context.MatchPlayers.RemoveRange(existingPlayers);

        for (int i = 0; i < playerIds.Length; i++)
        {
            var player = await _context.Players.FindAsync(playerIds[i]);
            var newMatchPlayer = new MatchPlayerEntity
            {
                Id = i * -1,
                MatchId = matchId,
                PlayerId = player.Id,
                Name = player.NaamKort ?? "",
                Status = newStatus,
                Ranking = match.Competition == Competition.Vttl ? player.KlassementVttl : player.KlassementSporta,
                Home = match.IsHomeMatch ?? false,
                Position = i
            };
            _context.MatchPlayers.Add(newMatchPlayer);
        }
        await _context.SaveChangesAsync();
        var newMatch = await GetMatch(matchId);
        return newMatch;
    }
    #endregion

    #region Report & Comments
    public async Task<Match> UpdateReport(MatchReport report)
    {
        var existingMatch = await _context.Matches.FirstAsync(x => x.Id == report.MatchId);
        existingMatch.ReportPlayerId = report.PlayerId;
        existingMatch.Description = report.Text;
        await _context.SaveChangesAsync();
        var newMatch = await GetMatch(report.MatchId);
        return newMatch;
    }

    public async Task<Match> AddComment(MatchComment comment)
    {
        var entity = _mapper.Map<MatchCommentEntity>(comment);
        entity.PostedOn = TtcDbContext.GetCurrentBelgianDateTime();
        _context.MatchComments.Add(entity);
        await _context.SaveChangesAsync();
        var newMatch = await GetMatch(comment.MatchId);
        return newMatch;
    }

    public async Task<Match> DeleteComment(int commentId)
    {
        var comment = await _context.MatchComments.SingleAsync(x => x.Id == commentId);
        _context.MatchComments.Remove(comment);
        await _context.SaveChangesAsync();

        return await GetMatch(comment.MatchId);
    }
    #endregion

    #region Frenoy Sync
    public async Task<OtherMatch> FrenoyOtherMatchSync(int matchId)
    {
        await FrenoyMatchSyncCore(matchId, false);
        var match = _context.Matches
            .WithIncludes()
            .Single(x => x.Id == matchId);
        return _mapper.Map<MatchEntity, OtherMatch>(match);
    }

    public async Task<Match> FrenoyMatchSync(int matchId, bool forceSync)
    {
        await FrenoyMatchSyncCore(matchId, forceSync);
        return await GetMatch(matchId);
    }

    public async Task FrenoyTeamSync(int teamId)
    {
        var team = await _context.Teams.SingleAsync(x => x.Id == teamId);
        var frenoySync = new FrenoyMatchesApi(_context, Constants.NormalizeCompetition(team.Competition));
        await frenoySync.SyncTeamMatches(team);
    }

    private async Task FrenoyMatchSyncCore(int matchId, bool forceSync)
    {
        var match = await _context.Matches
            .WithIncludes()
            .Include(x => x.HomeTeam)
            .Include(x => x.AwayTeam)
            .SingleAsync(x => x.Id == matchId);

        if (forceSync || (match.Date < DateTime.Now && !match.IsSyncedWithFrenoy))
        {
            var frenoySync = new FrenoyMatchesApi(_context, match.Competition, forceSync);
            await frenoySync.SyncMatchDetails(match);
        }
    }
    #endregion
    #endregion

    #region Excel Export
    public async Task<(byte[] file, SportaMatchFileInfo fileInfo)> GetExcelExport(int matchId, bool fillInOurTeam = true)
    {
        var exceller = await CreateExcelCreator(matchId);
        return (exceller.Create(fillInOurTeam), exceller.FileInfo);
    }

    private async Task<SportaMatchExcelCreator> CreateExcelCreator(int matchId)
    {
        int currentSeason = _context.CurrentSeason;
        var activePlayers = await _context.Players
            .Where(x => x.Gestopt == null)
            .Where(x => x.ClubIdSporta.HasValue)
            .ToArrayAsync();

        var match = await _context.Matches
            .Include(x => x.HomeTeam)
            .Include(x => x.AwayTeam)
            .Include(x => x.Players)
            .SingleAsync(x => x.Id == matchId);

        var teams = await _context.Teams
            .Include(x => x.Opponents.Select(o => o.Club))
            .Where(x => x.Year == currentSeason)
            .Where(x => x.Competition == Competition.Sporta.ToString())
            .ToArrayAsync();

        var frenoy = new FrenoyPlayersApi(_context, Competition.Sporta);
        var theirClubId = match.AwayTeamId.HasValue ? match.HomeClubId : match.AwayClubId;
        var opponentPlayers = await frenoy.GetPlayers(theirClubId);

        var exceller = new SportaMatchExcelCreator(_context, match, activePlayers, teams, opponentPlayers);
        return exceller;
    }
    #endregion
}
