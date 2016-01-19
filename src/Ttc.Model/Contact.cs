namespace Ttc.Model
{
    public class Contact
    {
        public string Address { get; set; }
        public string City { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }

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

        public override string ToString()
        {
            return $"Address={Address}, City={City}, Mobile={Mobile}, Email={Email}";
        }
    }
}