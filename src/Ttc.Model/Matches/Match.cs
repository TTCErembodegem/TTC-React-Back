using System;
using System.Collections.Generic;
using Ttc.Model.Teams;

namespace Ttc.Model.Matches
{
    public class Match
    {
        #region Kalender Properties
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string FrenoyMatchId { get; set; }
        public bool IsHomeMatch { get; set; }
        public int Week { get; set; }
        public int TeamId { get; set; }
        public OpposingTeam Opponent { get; set; }
        #endregion

        #region Verslag Properties
        public int ReportPlayerId { get; set; }
        public MatchScore Score { get; set; }
        public MatchOutcome ScoreType { get; set; }
        public string Description { get; set; }
        public bool IsPlayed { get; set; } // TODO: not sure if we still need this prop?
        public ICollection<MatchPlayer> Players { get; set; }
        public ICollection<MatchGame> Games { get; set; }
        #endregion

        public Match()
        {
            Players = new List<MatchPlayer>();
            Games = new List<MatchGame>();
            ScoreType = MatchOutcome.NotYetPlayed;
            Score = new MatchScore();
        }

        public Match(int playerId) : this()
        {
            ReportPlayerId = playerId;
        }

        public override string ToString() => $"Id={Id} on {Date.ToString("g")}, Home={IsHomeMatch}, TeamId={TeamId}, Opponent=({Opponent})";
    }
}