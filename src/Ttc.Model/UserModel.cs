using System.Collections.Generic;

namespace Ttc.Model
{
    public class User
    {
        public int PlayerId { get; set; }
        public ICollection<int> Teams { get; set; }
        public ICollection<string> Security { get; set; }

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
    }
}