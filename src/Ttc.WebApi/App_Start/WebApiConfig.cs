using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Owin;

namespace Ttc.WebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            config.EnableCors();

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{Id}",
                defaults: new { Id = RouteParameter.Optional }
            );
        }
    }
}
