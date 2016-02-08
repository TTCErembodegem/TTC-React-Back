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

        [ForeignKey("KalenderId")]
        public Verslag Verslag { get; set; }
        public int KalenderId { get; set; }

        public int MatchNummer { get; set; }
        public int ThuisSpelerUniqueIndex { get; set; }
        public int UitSpelerUniqueIndex { get; set; }
        public int ThuisSpelerSets { get; set; }
        public int UitSpelerSets { get; set; }
        public WalkOver WalkOver { get; set; }

        public override string ToString() => $"KalenderId={KalenderId}, Match#={MatchNummer}, ThuisSpeler={ThuisSpelerUniqueIndex}:{ThuisSpelerSets}, UitSpeler={UitSpelerUniqueIndex}:{UitSpelerSets}";
    }
}
