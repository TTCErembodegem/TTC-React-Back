using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Ttc.WebApi.Utilities;

namespace Ttc.WebApi.Controllers
{
    public class User
    {
        public int PlayerId { get; set; }
    }

    [AllowAnonymous]
    public class UsersController : BaseController
    {
        public User Get()
        {
            // Heh heh. This will have to be replaced with something more.. dynamic :p
            return new User
            {
                PlayerId = 20
            };
        }

        [HttpPost]
        public string LegacyLogin(string username, string password)
        {
            // header: login link
            // new page:
            // - login with Google/Facebook/...
            // - existing password

            // Login: is searchable players dropdownlist (chosen?) 

            // like stackoverflow.com

            return "";
        }
    }
}