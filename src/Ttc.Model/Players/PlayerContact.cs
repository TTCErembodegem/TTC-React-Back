namespace Ttc.Model.Players;

public class PlayerContact
{
    #region Properties
    public int PlayerId { get; set; }
    public string Email { get; set; }
    public string Mobile { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    #endregion

    #region Constructors
    public PlayerContact()
    {

    }

    public PlayerContact(int playerId, string eMail, string mobile, string address, string city)
    {
        PlayerId = playerId;
        Email = eMail;
        Mobile = mobile;
        Address = address;
        City = city;
    }
    #endregion

    public override string ToString() => $"PlayerId={PlayerId}, EMail={Email}, Mobile={Mobile}, Address={Address}, City={City}";
}
