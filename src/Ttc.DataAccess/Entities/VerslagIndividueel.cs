using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ttc.Model.Matches;

namespace Ttc.DataAccess.Entities
{
    internal class VerslagIndividueel
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("MatchId")]
        public Verslag Verslag { get; set; }
        public int MatchId { get; set; }

        public int MatchNumber { get; set; }
        public int HomePlayerUniqueIndex { get; set; }
        public int HomePlayerSets { get; set; }
        public int OutPlayerUniqueIndex { get; set; }
        public int OutPlayerSets { get; set; }
        public WalkOver WalkOver { get; set; }

        public override string ToString() => $"KalenderId={MatchId}, Match#={MatchNumber}, ThuisSpeler={HomePlayerUniqueIndex}:{OutPlayerUniqueIndex}, UitSpeler={HomePlayerSets}:{OutPlayerSets}";
    }
}
