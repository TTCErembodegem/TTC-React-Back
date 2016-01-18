using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataAccess.Entities
{
    internal class ClubPloeg
    {
        [Key]
        public int Id { get; set; }

        // Foreign Key voor Reeks
        public int? ReeksId { get; set; }

        // Foreign Key voor Club
        public int? ClubId { get; set; }

        //[ForeignKey("ClubId")]
        //public Club Club { get; set; }

        //[ForeignKey("ReeksId")]
        //public Reeks Reeks { get; set; }

        public string Code { get; set; }
    }
}
