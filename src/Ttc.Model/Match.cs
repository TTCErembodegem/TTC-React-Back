using System;

namespace Ttc.Model
{
    public class Match
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string FrenoyMatchId { get; set; }

        public bool IsHomeMatch { get; set; }
        public int Week { get; set; }

        // TODO vanaf hier
        public int? ThuisClubId { get; set; }
        public string ThuisPloeg { get; set; }
        public int? ThuisClubPloegId { get; set; }

        public int? UitClubId { get; set; }
        public string UitPloeg { get; set; }
        public int? UitClubPloegId { get; set; }
    }
}