using System.ComponentModel.DataAnnotations;

namespace Ttc.DataAccess.Backup
{
    public class BackupReport
    {
        [Key]
        public int Id { get; set; }
        public string FrenoyMatchId { get; set; }
        public string Description { get; set; }
        public int PlayerId { get; set; }

        public BackupReport(string frenoyMatchId, string description, int playerId)
        {
            FrenoyMatchId = frenoyMatchId;  
            Description = description;
            PlayerId = playerId;
        }

        public override string ToString()
        {
            return $"FrenoyMatchId: {FrenoyMatchId}, Description: {Description}, PlayerId: {PlayerId}";
        }
    }
}