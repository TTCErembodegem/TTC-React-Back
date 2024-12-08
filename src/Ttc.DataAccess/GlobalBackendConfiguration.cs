using AutoMapper;
using SimpleInjector;
using Ttc.DataAccess.Utilities;

namespace Ttc.DataAccess
{
    /// <summary>
    /// TTC Aalst DataAccess configuration
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