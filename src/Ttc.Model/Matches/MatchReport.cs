using System.Collections.Generic;

namespace Ttc.Model.Matches
{
    public class MatchReport
    {
        #region Properties
        public int PlayerId { get; set; }
        public MatchScore Score { get; set; }
        public MatchOutcome ScoreType { get; set; }
        public string Description { get; set; }
        public bool IsPlayed { get; set; }

        public ICollection<MatchPlayer> Players { get; set; }
        public ICollection<MatchGame> Games { get; set; }
        #endregion

        #region Constructors
        public MatchReport()
        {
        }

        public MatchReport(int playerId)
        {
            Players = new List<MatchPlayer>();
            Games = new List<MatchGame>();
            ScoreType = MatchOutcome.NotYetPlayed;
            Score = new MatchScore();
            PlayerId = playerId;
        }
        #endregion

        public override string ToString() =>  $"Score={Score}, ScoreType={ScoreType}";

    }

    public class MatchScore
    {
        #region Properties
        public int Home { get; set; }
        public int Out { get; set; }
        #endregion

        #region Constructors
        public MatchScore()
        {
        }

        public MatchScore(int homeScore, int outScore)
        {
            Home = homeScore;
            Out = outScore;
        }
        #endregion

        public override string ToString() => $"{Home}-{Out}";
    }

    public enum MatchOutcome
    {
        NotYetPlayed,
        Won,
        Lost,
        Draw,
        WalkOver
    }
}