using System;
using Ttc.Model.Teams;

namespace Ttc.Model.Matches
{
    public class Match
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string FrenoyMatchId { get; set; }

        public bool IsHomeMatch { get; set; }
        public int Week { get; set; }

        public int ReeksId { get; set; }
        public OpposingTeam Opponent { get; set; }

        public MatchReport Report { get; set; }

        public override string ToString() => $"Id={Id} on {Date.ToString("g")}, Home={IsHomeMatch}, ReeksId={ReeksId}, Opponent=({Opponent})";
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
}