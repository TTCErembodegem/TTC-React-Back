namespace Ttc.Model
{
    public class Contact
    {
        #region Properties
        public string Address { get; set; }
        public string City { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        #endregion

        #region Constructors
        public Contact()
        {

        }

        public Contact(string address, string city, string mobile, string email)
        {
            Address = address;
            City = city;
            Mobile = mobile;
            Email = email;
        }
        #endregion

        public override string ToString() => $"Address={Address}, City={City}, Mobile={Mobile}, Email={Email}";
    }
}