using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Ttc.WebApi.Controllers
{
    [AllowAnonymous]
    public class UsersController : ApiController
    {
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