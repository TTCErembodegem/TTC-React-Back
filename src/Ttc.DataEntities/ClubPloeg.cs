﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataEntities
{
    public class ClubPloeg
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ReeksId")]
        public Reeks Reeks { get; set; }
        public int? ReeksId { get; set; }
        
        [ForeignKey("ClubId")]
        public ClubEntity Club { get; set; }
        public int? ClubId { get; set; }

        public ICollection<Kalender> Matchen { get; set; }
        public ICollection<ClubPloegSpeler> Spelers { get; set; }

        /// <summary>
        /// Team A, B, C, ...
        /// </summary>
        public string Code { get; set; }

        public override string ToString() => $"Id={Id}, Reeks=_{Reeks}_, ClubId={ClubId}, TeamCode={Code}";
    }
}