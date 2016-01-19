namespace Ttc.Model
{
    public class Contact
    {
        public string Address { get; }
        public string City { get; }
        public string Mobile { get; }
        public string Email { get; }

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