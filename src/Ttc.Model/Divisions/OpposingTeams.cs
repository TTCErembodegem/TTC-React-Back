namespace Ttc.Model.Divisions
{
    public class OpposingTeams
    {
        /// <summary>
        /// Team A, B, C, ...
        /// </summary>
        public string TeamCode { get; set; }

        public int ClubId { get; set; }

        public override string ToString()
        {
            return $"ClubId={ClubId}, Team={TeamCode}";
        }
    }
}