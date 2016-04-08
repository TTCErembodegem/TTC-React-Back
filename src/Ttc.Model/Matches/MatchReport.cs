using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ttc.Model.Matches
{
    public class MatchReport
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public string Text { get; set; }
        public int PlayerId { get; set; }
        public DateTime PostedOn { get; set; }

        public override string ToString() => $"MatchId: {MatchId}, Text: {Text}, PlayerId: {PlayerId}";
    }
}
