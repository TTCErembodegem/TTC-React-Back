using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Ttc.Model.Core;
using Ttc.Model.Teams;

namespace Ttc.Model.Matches
{
    public interface IMatch
    {
        bool ShouldHide();
    }

    /// <summary>
    /// Non-TTC Erembodegem match
    /// </summary>
    public class OtherMatch : IMatch
    {
        #region Properties
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public bool ShouldBePlayed { get; set; }
        public string FrenoyMatchId { get; set; }
        public bool IsSyncedWithFrenoy { get; set; }
        public int Week { get; set; }
        public string Competition { get; set; }
        public int FrenoyDivisionId { get; set; }

        public OpposingTeam Home { get; set; }
        public OpposingTeam Away { get; set; }

        public MatchScore Score { get; set; }
        public MatchOutcome ScoreType { get; set; }
        public bool IsPlayed { get; set; }
        [TtcConfidential("MATCH")]
        public ICollection<MatchPlayer> Players { get; set; }
        public ICollection<MatchGame> Games { get; set; }
        #endregion

        public bool ShouldHide()
        {
            // Duplicated below
            return IsPlayed || IsSyncedWithFrenoy || Helpers.HasMatchStarted(Date);
        }

        public override string ToString() => $"Id: {Id}, Date: {Date}, FrenoyMatchId: {FrenoyMatchId}, IsSyncedWithFrenoy: {IsSyncedWithFrenoy}, Home: {Home}, Away: {Away}, Score: {Score}";
    }

    public class Match : IMatch
    {
        #region Kalender Properties
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public bool ShouldBePlayed { get; set; }
        public string FrenoyMatchId { get; set; }
        public bool IsSyncedWithFrenoy { get; set; }
        public int Week { get; set; }
        public string Competition { get; set; }
        public int FrenoyDivisionId { get; set; }
        [TtcConfidential]
        public string Block { get; set; }

        public int TeamId { get; set; }
        /// <summary>
        /// Null when TTC Erembodegem did not play (~ ReadonlyMatch)
        /// True/False: Was TTC Erembodegem, True=Was in Erembodegem
        /// </summary>
        public bool? IsHomeMatch { get; set; }
        public OpposingTeam Opponent { get; set; }
        #endregion

        #region Verslag Properties
        public int ReportPlayerId { get; set; }
        public MatchScore Score { get; set; }
        public MatchOutcome ScoreType { get; set; }
        public string Description { get; set; }
        public bool IsPlayed { get; set; }

        [TtcConfidential("MATCH")]
        public ICollection<MatchPlayer> Players { get; set; }
        [TtcConfidential]
        public string FormationComment { get; set; }
        public ICollection<MatchGame> Games { get; set; }
        [TtcConfidential("MATCH-COMMENTS")]
        public ICollection<MatchComment> Comments { get; set; }
        #endregion

        #region Constructors
        public Match()
        {
            Players = new List<MatchPlayer>();
            Games = new List<MatchGame>();
            Comments = new List<MatchComment>();
            ScoreType = MatchOutcome.NotYetPlayed;
            Score = new MatchScore();
        }

        public Match(int playerId) : this()
        {
            ReportPlayerId = playerId;
        }
        #endregion

        public bool ShouldHide()
        {
            // Duplicated above
            return IsPlayed || IsSyncedWithFrenoy || Helpers.HasMatchStarted(Date);
        }

        public override string ToString() => $"Id={Id} on {Date:g}, Home={IsHomeMatch}, TeamId={TeamId}, Opponent=({Opponent})";
    }
}