namespace Ttc.Model.Teams
{
    public class OpposingTeam
    {
        #region Properties
        /// <summary>
        /// Team A, B, C, ...
        /// </summary>
        public string TeamCode { get; set; }

        public int ClubId { get; set; }
        #endregion

        public override string ToString() => $"ClubId={ClubId}, Team={TeamCode}";
    }
}