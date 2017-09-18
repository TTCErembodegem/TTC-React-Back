namespace Ttc.Model.Teams
{
    public class DivisionRanking
    {
        public int Position { get; set; }
        public int GamesWon { get; set; }
        public int GamesLost { get; set; }
        public int GamesDraw { get; set; }
        public int Points { get; set; }
        public int ClubId { get; set; }
        public string TeamCode { get; set; }
        public bool IsForfait { get; set; }
    }
}