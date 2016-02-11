using System.Collections.Generic;

namespace Ttc.Model.Matches
{
    public class MatchReport
    {
        public int PlayerId { get; set; }
        public MatchScore Score { get; set; }
        public MatchOutcome ScoreType { get; set; }
        public string Description { get; set; }

        public ICollection<MatchPlayer> Players { get; set; }
        public ICollection<MatchGame> Games { get; set; }

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

        public override string ToString()
        {
            return $"Score={Score}, ScoreType={ScoreType}";
        }
    }

    public class MatchScore
    {
        public int Home { get; set; }
        public int Out { get; set; }

        public MatchScore()
        {
        }

        public MatchScore(int homeScore, int outScore)
        {
            Home = homeScore;
            Out = outScore;
        }

        public override string ToString()
        {
            return $"{Home}-{Out}";
        }
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