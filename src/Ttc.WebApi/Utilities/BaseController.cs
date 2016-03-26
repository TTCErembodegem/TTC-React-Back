using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Cors;
using Ttc.Model.Core;
using Ttc.WebApi.Utilities.Auth;

namespace Ttc.WebApi.Utilities
{
    [EnableCors("*", "*", "*")]
    [TtcAuthorizationFilter]
    public class BaseController : ApiController
    {

        private bool HasValidToken()
        {
            if (Request.Headers.Authorization == null)
            {
                return false;
            }

            var token = Request.Headers.Authorization.Parameter;
            var user = TtcAuthorizationFilterAttribute.ValidateToken(token);
            return user != null;
        }

        protected void CleanSensitiveData(IEnumerable<object> data)
        {
            if (!HasValidToken())
            {
                foreach (var record in data)
                {
                    CleanSensitiveDataCore(record);
                }
            }
        }

        protected void CleanSensitiveData(object data)
        {
            if (!HasValidToken())
            {
                CleanSensitiveDataCore(data);
            }
        }

        private static void CleanSensitiveDataCore(object data)
        {
            var props = data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
            foreach (var prop in props)
            {
                var toHide = prop.GetCustomAttribute<TtcConfidentialAttribute>();
                if (toHide != null)
                {
                    if (toHide.Strategy == null || toHide.Strategy.ShouldHide(data))
                    {
                        prop.SetValue(data, null, null);
                    }
                }
            }
        }
    }
}