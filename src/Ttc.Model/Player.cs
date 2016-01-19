using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ttc.Model
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public bool IsActive { get; set; }

        public PlayerStyle Style { get; set; }
        public Contact Contact { get; set; }

        public PlayerCompetition Vttl { get; set; }
        public PlayerCompetition Sporta { get; set; }
    }
}
