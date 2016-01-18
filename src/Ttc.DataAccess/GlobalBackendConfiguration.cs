using AutoMapper;
using SimpleInjector;
using Ttc.DataAccess.Entities;
using Ttc.Model;

namespace Ttc.DataAccess
{
    public static class GlobalBackendConfiguration
    {
        public static void Configure()
        {
            ConfigureAutoMapper();
        }

        public static void ConfigureIoC(Container container)
        {

        }

        private static void ConfigureAutoMapper()
        {
            Mapper.CreateMap<Speler, Player>();
        }
    }
}