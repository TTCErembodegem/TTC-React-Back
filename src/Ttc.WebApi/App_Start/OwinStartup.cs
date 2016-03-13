using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Newtonsoft.Json.Serialization;
using Owin;
using SimpleInjector.Extensions.ExecutionContextScoping;
using SimpleInjector.Integration.WebApi;
using Ttc.DataAccess;

[assembly: OwinStartup(typeof(Ttc.WebApi.OwinStartup))]
namespace Ttc.WebApi
{
    public class OwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            var container = IoCInitializer.Initialize();
            app.Use(async (context, next) => {
                using (container.BeginExecutionContextScope())
                {
                    await next();
                }
            });

            HttpConfiguration config = new HttpConfiguration
            {
                DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container)
            };

            WebApiConfig.Register(config);
            ConfigureJson(config);
            GlobalBackendConfiguration.ConfigureAutoMapper();

            app.UseWebApi(config);
        }

        private static void ConfigureJson(HttpConfiguration config)
        {
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            //JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            //{
            //    ContractResolver = new CamelCasePropertyNamesContractResolver()
            //};
        }
    }
}