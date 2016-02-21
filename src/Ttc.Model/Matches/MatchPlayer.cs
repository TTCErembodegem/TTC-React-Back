namespace Ttc.Model.Matches
{
    public class MatchPlayer
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public int Position { get; set; }
        public string Name { get; set; }
        public string Ranking { get; set; }
        public int UniqueIndex { get; set; }
        public int Won { get; set; }

        /// <summary>
        /// True == TTC Erembodegem player
        /// </summary>
        public bool Home { get; set; }
        public int PlayerId { get; set; }
        public string Alias { get; set; }

        public override string ToString() =>  $"{Position} {Name} ({Ranking}), Won={Won}";
    }
}