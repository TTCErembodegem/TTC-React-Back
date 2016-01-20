using System.Runtime;

namespace Ttc.Model
{
    public class Team
    {
        /// <summary>
        /// Team A, B, C, ...
        /// </summary>
        public string Code { get; set; }

        public int ClubId { get; set; }

        //public int DivisionId { get; set; }
        //public Division Division { get; set; }

        public override string ToString()
        {
            return $"ClubId={ClubId}, Code={Code}";
        }
    }
}