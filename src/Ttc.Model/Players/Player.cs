using Ttc.Model.Core;

namespace Ttc.Model.Players
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public bool Active { get; set; }

        public PlayerStyle Style { get; set; }

        [TtcConfidential]
        public Contact Contact { get; set; }

        public PlayerCompetition Vttl { get; set; }
        public PlayerCompetition Sporta { get; set; }

        public override string ToString() => $"Id={Id}, Alias={Alias}, Active={Active}";

        public PlayerCompetition GetCompetition(Competition competition) => competition == Competition.Vttl ? Vttl : Sporta;
    }
}
