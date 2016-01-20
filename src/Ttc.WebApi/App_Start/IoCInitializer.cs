using System.Web.Http;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using Ttc.DataAccess;
using Ttc.DataAccess.Services;
using Ttc.WebApi;

[assembly: WebActivator.PostApplicationStartMethod(typeof(IoCInitializer), "Initialize")]

namespace Ttc.WebApi
{
    public static class IoCInitializer
    {
        public static void Initialize()
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new WebApiRequestLifestyle();
            
            InitializeContainer(container);
            container.RegisterWebApiControllers(GlobalConfiguration.Configuration);
            container.Verify();

            GlobalBackendConfiguration.ConfigureIoC(container);

            GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
        }
     
        private static void InitializeContainer(Container container)
        {
            container.Register<PlayerService, PlayerService>(Lifestyle.Scoped);
            container.Register<ConfigService, ConfigService>(Lifestyle.Scoped);
            container.Register<ClubService, ClubService>(Lifestyle.Scoped);
            container.Register<TeamService, TeamService>(Lifestyle.Scoped);
        }
    }
}