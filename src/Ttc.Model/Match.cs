using System;
using Ttc.Model.Teams;

namespace Ttc.Model
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

        public override string ToString()
        {
            return $"Id={Id} on {Date.ToString("g")}, Home={IsHomeMatch}, ReeksId={ReeksId}, Opponent=({Opponent})";
        }
    }
}