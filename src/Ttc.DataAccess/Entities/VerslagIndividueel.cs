using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ttc.DataAccess.Entities
{
    internal class VerslagIndividueel
    {
        [Key]
        public int Id { get; set; }
        public int VerslagId { get; set; }
        //[ForeignKey("VerslagId")]
        //public Verslag Verslag { get; set; }
        public int MatchNummer { get; set; }
        public int ThuisSpelerUniqueIndex { get; set; }
        public int UitSpelerUniqueIndex { get; set; }
        public int ThuisSpelerSets { get; set; }
        public int UitSpelerSets { get; set; }
        public WalkOver WalkOver { get; set; }

        public override string ToString() =>$"VerslagId={VerslagId}, Match#={MatchNummer}, ThuisSpeler={ThuisSpelerUniqueIndex}:{ThuisSpelerSets}, UitSpeler={UitSpelerUniqueIndex}:{UitSpelerSets}";
    }

    internal enum WalkOver
    {
        None,
        Thuis,
        Uit
    }
}
