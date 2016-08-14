using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;
using log4net;

namespace Ttc.WebApi.Utilities
{
    public class TtcExceptionFilterAttribute: ExceptionFilterAttribute
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ExceptionFilterAttribute));

        public override void OnException(HttpActionExecutedContext context)
        {
            if (TooManyConnections(context.Exception))
            {
                context.Response = new HttpResponseMessage(HttpStatusCode.RequestTimeout)
                {
                    Content = new StringContent(TooManyConnectionsExMessage),
                    ReasonPhrase = TooManyConnectionsExMessage
                };
            }
            else
            {
                Logger.Error(context.Exception.ToString());

                context.Response = new HttpResponseMessage(HttpStatusCode.Accepted)
                {
                    Content = new StringContent(context.Exception.ToString()),
                    ReasonPhrase = "Something went terribly wrong!"
                };
            }
        }

        private const string TooManyConnectionsExMessage = "Too many connections";
        private const string OtherExMessage = "has exceeded the 'max_user_connections' resource";
        private static bool TooManyConnections(Exception ex)
        {
            Func<string, bool> isTooManyConnections = msg => msg == TooManyConnectionsExMessage || msg.Contains(OtherExMessage);

            string fullErrorName = ex.GetType().FullName;
            if (fullErrorName == "System.Data.Entity.Core.EntityException")
            {
                if (ex.InnerException != null) 
                {
                    if (isTooManyConnections(ex.InnerException.Message))
                    {
                        return true;
                    }

                    if (ex.InnerException.InnerException != null && isTooManyConnections(ex.InnerException.InnerException.Message))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}