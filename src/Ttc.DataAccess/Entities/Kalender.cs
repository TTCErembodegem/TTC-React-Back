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
        public TimeSpan Uur { get; set; }

        public int? Thuis { get; set; }
        public int? Week { get; set; }
        public string FrenoyMatchId { get; set; }

        /// <summary>
        /// De ThuisClubId is altijd TTC Erembodegem
        /// (don't ask!)
        /// </summary>
        public int? ThuisClubId { get; set; }

        /// <summary>
        /// Team A, B, C, ...
        /// </summary>
        public string ThuisPloeg { get; set; }

        /// <summary>
        /// De inhoud van ThuisClubPloegId is ClubId en PloegCode
        /// dat ook in Kalender zit. Deze extra layer is dus niet
        /// nodig voor de Kalender op zich
        /// </summary>
        public int? ThuisClubPloegId { get; set; }

        public int? UitClubId { get; set; }
        public string UitPloeg { get; set; }
        public int? UitClubPloegId { get; set; }

        // Voor matchen zijn deze twee niet ingevuld:
        public string Beschrijving { get; set; }
        public string GeleideTraining { get; set; }

        public override string ToString()
        {
            var str = $"Id={Id}, Date={Datum.ToString("d")}{Uur.ToString("hh:MM")}";
            if (!Thuis.HasValue)
            {
                return $"{str}, {Beschrijving} / {GeleideTraining}";
            }
            return $"{str}, Thuis={Thuis}, Ploeg={ThuisPloeg}, UitClubId={UitClubId}, UitPloeg={UitPloeg}";
        }
    }
}
