using System.Web.Http;

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

            config.Routes.MapHttpRoute(
                name: "ActionApi",
                routeTemplate: "api/{controller}/{action}",
                defaults: new { }
           );

           // config.Routes.MapHttpRoute(
           //     name: "UploadApi",
           //     routeTemplate: "api/upload/{action}/{id}",
           //     defaults: new {}
           //);
        }
    }
}
