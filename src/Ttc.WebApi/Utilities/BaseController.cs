using System.Web.Http;
using System.Web.Http.Cors;
using Ttc.WebApi.Utilities.Auth;

namespace Ttc.WebApi.Utilities
{
    [EnableCors("*", "*", "*")]
    [TtcAuthorizationFilter]
    public class BaseController : ApiController
    {

    }
}