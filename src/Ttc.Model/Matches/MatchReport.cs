using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ttc.Model.Matches
{
    public class MatchReport
    {
        public int MatchId { get; set; }
        public int PlayerId { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return $"MatchId: {MatchId}, PlayerId: {PlayerId}, Text: {Text}";
        }
    }

    public class MatchComment
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public string Text { get; set; }
        public int PlayerId { get; set; }
        public DateTime PostedOn { get; set; }
        public bool Hidden { get; set; }
        public string ImageUrl { get; set; }

        public override string ToString() => $"MatchId: {MatchId}, Text: {Text}, PlayerId: {PlayerId}";
    }
}
