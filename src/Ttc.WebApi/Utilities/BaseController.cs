using System.Web.Http;
using System.Web.Http.Cors;

namespace Ttc.WebApi.Utilities
{
    [EnableCors("*", "*", "*")]
    public class BaseController : ApiController
    {

    }
}