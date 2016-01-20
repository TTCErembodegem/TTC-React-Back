namespace Ttc.Model.Players
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

        public override string ToString()
        {
            return $"Id={Id}, Alias={Alias}, IsActive={IsActive}";
        }
    }
}
