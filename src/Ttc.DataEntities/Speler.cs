﻿using System.ComponentModel.DataAnnotations;

namespace Ttc.DataEntities
{
    /// <summary>
    /// Mapped to db.
    /// </summary>
    public class Speler
    {
        [Key]
        public int Id { get; set; }
        public string Naam { get; set; }
        public string NaamKort { get; set; }
        public string Stijl { get; set; }
        public string BesteSlag { get; set; }

        #region Vttl
        public int? ClubIdVttl { get; set; }
        public int? IndexVttl { get; set; }
        public string LinkKaartVttl { get; set; }
        public string KlassementVttl { get; set; }
        public int? ComputerNummerVttl { get; set; }
        public int? VolgnummerVttl { get; set; }
        #endregion

        #region Sporta
        public int? ClubIdSporta { get; set; }
        public int? IndexSporta { get; set; }
        public string LinkKaartSporta { get; set; }
        public string KlassementSporta { get; set; }
        public int? LidNummerSporta { get; set; }
        public int? VolgnummerSporta { get; set; }
        #endregion

        #region Address
        public string Adres { get; set; }
        public string Gemeente { get; set; }
        public string Gsm { get; set; }
        public string Email { get; set; }
        #endregion

        /// <summary>
        /// Jaar gestopt. <see cref="IsGestopt"/>?
        /// </summary>
        public int? Gestopt { get; set; }
        public bool IsGestopt => Gestopt != null;

        public bool IsFromOwnClub() => ClubIdSporta == Constants.OwnClubId || ClubIdVttl == Constants.OwnClubId;

        public override string ToString() => $"Id={Id}, Alias={NaamKort} ({ClubIdVttl}, {ClubIdSporta}), Vttl={KlassementVttl}, Sporta={KlassementSporta}, IsGestopt={IsGestopt}";
    }
}