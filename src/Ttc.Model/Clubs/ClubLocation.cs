namespace Ttc.Model.Clubs
{
    public class ClubLocation
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public Contact Contact { get; set; }

        public override string ToString() =>  $"Id={Id}, Desc={Description}, Loc={Contact.Address}";
    }
}