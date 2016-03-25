using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Threading;

namespace Ttc.DataEntities
{
    [Table("matchcomment")]
    public class MatchCommentEntity
    {
        [Key]
        public int Id { get; set; }
        public DateTime PostedOn { get; set; }
        public string Text { get; set; }

        [ForeignKey("MatchId")]
        public MatchEntity Match { get; set; }
        public int MatchId { get; set; }

        //[ForeignKey("PlayerId")]
        //public PlayerEntity Player { get; set; }
        public int PlayerId { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}, Text: {Text}, MatchId: {MatchId}, PlayerId: {PlayerId}";
        }
    }
}