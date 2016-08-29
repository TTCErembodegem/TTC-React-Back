using System.Globalization;
using System.Web.Http;
using log4net;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.Cookies;
using Newtonsoft.Json.Serialization;
using Owin;
using SimpleInjector.Extensions.ExecutionContextScoping;
using SimpleInjector.Integration.WebApi;
using Ttc.DataAccess;
using Ttc.WebApi.Utilities;

[assembly: OwinStartup(typeof(Ttc.WebApi.OwinStartup))]
namespace Ttc.WebApi
{
    public class OwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("nl-BE");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("nl-BE");

            log4net.Config.XmlConfigurator.Configure();
            var logger = LogManager.GetLogger(typeof(OwinStartup));
            logger.Info("Starting up...");

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

            app.Map("/signalr", map =>
            {
                map.UseCors(CorsOptions.AllowAll);
                map.RunSignalR(new HubConfiguration() {EnableDetailedErrors = true, EnableJSONP = true});
            });

            config.Filters.Add(new TtcExceptionFilterAttribute());

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