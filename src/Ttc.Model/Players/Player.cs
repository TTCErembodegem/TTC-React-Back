using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ttc.Model.Core;

namespace Ttc.Model.Players
{
    public class Player
    {
        #region Properties
        public int Id { get; set; }
        [StringLength(100)]
        public string FirstName { get; set; }
        [StringLength(100)]
        public string LastName { get; set; }

        public string Alias { get; set; }
        public bool Active { get; set; }
        public int? QuitYear { get; set; }

        [TtcConfidential]
        public string Security { get; set; }
        [TtcConfidential]
        public bool HasKey { get; set; }

        public PlayerStyle Style { get; set; }

        [TtcConfidential]
        public PlayerContact Contact { get; set; }

        public PlayerCompetition Vttl { get; set; }
        public PlayerCompetition Sporta { get; set; }
        #endregion

        public override string ToString() => $"Id={Id}, Alias={Alias}, Active={Active}";

        public PlayerCompetition GetCompetition(Competition competition) => competition == Competition.Vttl ? Vttl : Sporta;
    }
}
