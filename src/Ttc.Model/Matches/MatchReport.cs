namespace Ttc.Model.Matches
{
    public class MatchReport
    {
        public int KalenderId { get; set; }
        public int PlayerId { get; set; }
        public MatchScore Score { get; set; }
        public MatchOutcome ScoreType { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return $"KalenderId={KalenderId}, Score={Score}, ScoreType={ScoreType}";
        }
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