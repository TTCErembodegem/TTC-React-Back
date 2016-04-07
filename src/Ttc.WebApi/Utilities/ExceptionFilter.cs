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
            Logger.Error(context.Exception.ToString());

            context.Response = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent(context.Exception.ToString()),
                ReasonPhrase = "Something went terribly wrong!"
            };
        }
    }
}