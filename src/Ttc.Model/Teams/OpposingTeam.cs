namespace Ttc.Model.Teams
{
    public class OpposingTeam
    {
        /// <summary>
        /// Team A, B, C, ...
        /// </summary>
        public string TeamCode { get; set; }

        public int ClubId { get; set; }

        public override string ToString() => $"ClubId={ClubId}, Team={TeamCode}";
    }
}