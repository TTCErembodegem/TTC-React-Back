using System.Reflection;
using System.Security.Principal;
using System.Web.Http.Controllers;
using Ttc.Model;

namespace Ttc.WebApi.Utilities.Auth
{
    public class TtcPrincipal : IPrincipal
    {
        public IIdentity Identity { get; private set; }

        public TtcPrincipal(User user)
        {
            Identity = new TtcIdentity(user);
        }

        public bool IsInRole(string role)
        {
            // TODO: TtcPrincipal IsInRole returns hardcoded false
            return false;
        }
    }

    public class TtcIdentity : IIdentity
    {
        public User User { get; private set; }
        public string AuthenticationType => "User";
        public bool IsAuthenticated => true;
        string IIdentity.Name => User.Alias;

        public TtcIdentity(User user)
        {
            User = user;
        }

        public override string ToString()
        {
            return User.ToString();
        }
    }
}