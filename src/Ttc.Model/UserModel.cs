using System.Collections.Generic;

namespace Ttc.Model
{
    public class User
    {
        public int PlayerId { get; set; }
        public string Alias { get; set; }
        public ICollection<int> Teams { get; set; }
        public ICollection<string> Security { get; set; }
        public string Token { get; set; }

        public User()
        {
            Teams = new List<int>();
            Security = new List<string>();
        }

        public override string ToString() => $"PlayerId={PlayerId}, Teams='{string.Join(", ", Teams)}', Security='{string.Join(", ", Security)}'";
    }

    public class UserCredentials
    {
        public int PlayerId { get; set; }
        public string Password { get; set; }

        public override string ToString()
        {
            return $"PlayerId: {PlayerId}, Password: {Password}";
        }
    }
}