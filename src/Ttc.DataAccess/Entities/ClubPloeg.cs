using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataAccess.Entities
{
    internal class ClubPloeg
    {
        [Key]
        public int Id { get; set; }
        public int? ReeksId { get; set; }
        public Reeks Reeks { get; set; }
        public int? ClubId { get; set; }

        //[ForeignKey("ClubId")]
        //public Club Club { get; set; }

        /// <summary>
        /// Team A, B, C, ...
        /// </summary>
        public string Code { get; set; }
    }
}
