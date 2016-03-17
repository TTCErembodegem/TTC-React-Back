using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;

namespace Ttc.WebApi.Utilities
{
    public class TtcExceptionFilterAttribute: ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            context.Response = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent(context.Exception.ToString()),
                ReasonPhrase = "Something went terribly wrong!"
            };
        }
    }
}