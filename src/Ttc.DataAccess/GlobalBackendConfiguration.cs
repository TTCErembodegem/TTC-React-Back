using AutoMapper;
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

        private static void ConfigureAutoMapper()
        {
            Mapper.CreateMap<Speler, Player>();
        }
    }
}