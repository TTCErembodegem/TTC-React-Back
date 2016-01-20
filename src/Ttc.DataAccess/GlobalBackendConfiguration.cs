using System;
using System.Diagnostics;
using System.Linq.Expressions;
using SimpleInjector;
using Ttc.DataAccess.App_Start;
using Ttc.DataAccess.Utilities;

namespace Ttc.DataAccess
{
    public static class GlobalBackendConfiguration
    {
        public static void Configure()
        {
            AutoMapperConfig.Configure(new KlassementValueConverter());
        }

        public static void ConfigureIoC(Container container)
        {
            // any backend IoC mapping can happen here...
        }
    }
}