using AutoMapper;
using SimpleInjector;
using Ttc.DataAccess.Utilities;

namespace Ttc.DataAccess
{
    /// <summary>
    /// TTC Erembodegem DataAccess configuration
    /// </summary>
    public static class GlobalBackendConfiguration
    {
        public static void ConfigureAutoMapper()
        {
            AutoMapperConfig.Configure(new KlassementValueConverter());
        }

        public static void ConfigureIoC(Container container)
        {
            // any backend IoC mapping can happen here...
        }
    }
}