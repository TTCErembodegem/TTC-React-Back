namespace Ttc.Model.Clubs;

public class ClubLocation
{
    #region Properties
    public int Id { get; set; }
    public string Description { get; set; }
    public string Address { get; set; }
    public string PostalCode { get; set; }
    public string City { get; set; }
    public string Mobile { get; set; }
    #endregion

    #region Constructor
    public ClubLocation()
    {
        Description = "";
        Address = "";
        PostalCode = "";
        City = "";
        Mobile = "";
    }
    #endregion

    public override string ToString() =>  $"Id={Id}, Desc={Description}, Loc={Address}";
}
