using System.Collections.Generic;

namespace Ttc.Model.Matches
{
    public class MatchPlayer
    {
        public int Position { get; set; }
        public string Name { get; set; }
        public string Ranking { get; set; }
        public int UniqueIndex { get; set; }
        public int Won { get; set; }
        public bool Home { get; set; }
        public int? PlayerId { get; set; }

        public override string ToString() =>  $"{Position} {Name} ({Ranking}), Won={Won}";
    }
}