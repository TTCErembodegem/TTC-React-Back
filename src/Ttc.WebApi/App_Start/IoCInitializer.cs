using System.Globalization;
using System.Threading;
using System.Web.Http;
using SimpleInjector;
using SimpleInjector.Extensions.ExecutionContextScoping;
using Ttc.DataAccess;
using Ttc.DataAccess.Services;
using Ttc.WebApi;

[assembly: WebActivator.PostApplicationStartMethod(typeof(IoCInitializer), "Initialize")]

namespace Ttc.WebApi
{
    public static class IoCInitializer
    {
        public static Container Initialize()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("nl-BE");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("nl-BE");

            var container = new Container();
            container.Options.DefaultScopedLifestyle = new ExecutionContextScopeLifestyle();
            
            RegisterDependencies(container);
            container.RegisterWebApiControllers(GlobalConfiguration.Configuration);
            GlobalBackendConfiguration.ConfigureIoC(container);
            container.Verify();

            return container;
        }
     
        private static void RegisterDependencies(Container container)
        {
            container.Register<PlayerService, PlayerService>(Lifestyle.Scoped);
            container.Register<ConfigService, ConfigService>(Lifestyle.Scoped);
            container.Register<ClubService, ClubService>(Lifestyle.Scoped);
            container.Register<TeamService, TeamService>(Lifestyle.Scoped);
        }
    }
}