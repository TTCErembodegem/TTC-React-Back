using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using Ttc.DataAccess.Services;
using Ttc.Model;
using Ttc.WebApi.Utilities;

namespace Ttc.WebApi.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("api/users")]
    public class UsersController : BaseController
    {
        #region Constructor
        private readonly PlayerService _service;

        public UsersController(PlayerService service)
        {
            _service = service;
        }
        #endregion

        [HttpPost]
        //[Route("Login")]
        public User Login([FromBody]UserCredentials user)
        {
            var player = _service.Login(user);
            return player;
        }
    }
}