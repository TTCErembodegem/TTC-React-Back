using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataAccess.Entities
{
    [Table("kalender")]
    internal class Kalender
    {
        [Key]
        public int ID { get; set; }

        public DateTime Datum { get; set; }

        public DateTime Uur { get; set; }

        public int? ThuisClubID { get; set; }

        public string ThuisPloeg { get; set; }

        public int? ThuisClubPloegID { get; set; }

        public int? UitClubID { get; set; }

        public string UitPloeg { get; set; }

        public int? UitClubPloegID { get; set; }

        public int? Week { get; set; }

        public string Beschrijving { get; set; }

        public int? Thuis { get; set; }

        public string GeleideTraining { get; set; }

        public string FrenoyMatchId { get; set; }
    }
}
