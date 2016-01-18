using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataAccess.Entities
{
    internal class Kalender
    {
        [Key]
        public int Id { get; set; }

        public DateTime Datum { get; set; }

        public DateTime Uur { get; set; }

        public int? ThuisClubId { get; set; }

        public string ThuisPloeg { get; set; }

        public int? ThuisClubPloegId { get; set; }

        public int? UitClubId { get; set; }

        public string UitPloeg { get; set; }

        public int? UitClubPloegId { get; set; }

        public int? Week { get; set; }

        public string Beschrijving { get; set; }

        public int? Thuis { get; set; }

        public string GeleIdeTraining { get; set; }

        public string FrenoyMatchId { get; set; }
    }
}
