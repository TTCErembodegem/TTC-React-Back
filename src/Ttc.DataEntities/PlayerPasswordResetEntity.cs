using System.ComponentModel.DataAnnotations;

namespace Ttc.DataEntities;

public class PlayerPasswordResetEntity
{
    [Key]
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public DateTime ExpiresOn { get; set; }
    public int PlayerId { get; set; }

    public PlayerPasswordResetEntity()
    {

    }

    public PlayerPasswordResetEntity(int playerId)
    {
        Guid = Guid.NewGuid();
        ExpiresOn = DateTime.UtcNow.AddDays(2);
        PlayerId = playerId;
    }

    public override string ToString() => $"{nameof(Guid)}: {Guid}, {nameof(ExpiresOn)}: {ExpiresOn}, {nameof(PlayerId)}: {PlayerId}";
}
