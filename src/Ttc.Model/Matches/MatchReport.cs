namespace Ttc.Model.Matches
{
    public class MatchReport
    {
        public int KalenderId { get; set; }
        public int PlayerId { get; set; }
        public string Score { get; set; }
        public MatchOutcome ScoreType { get; set; }
        public string Description { get; set; }
    }

    public enum MatchOutcome
    {
        NotYetPlayed,
        Won,
        Lost,
        Draw,
        WalkOver
    }
}