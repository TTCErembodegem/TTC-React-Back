using System;
using System.Linq.Expressions;
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
            Mapper.CreateMap<Speler, Player>()
                .ForMember(
                    dest => dest.Name,
                    opts => opts.MapFrom(src => src.Naam))
                .ForMember(
                    dest => dest.Alias,
                    opts => opts.MapFrom(src => src.NaamKort))
                .ForMember(
                    dest => dest.IsActive,
                    opts => opts.MapFrom(src => !src.IsGestopt))
                .ForMember(
                    dest => dest.Style,
                    opts => opts.MapFrom(src => new PlayerStyle(src.Stijl, src.BesteSlag)))
                .ForMember(
                    dest => dest.Contact,
                    opts => opts.MapFrom(src => new Contact(src.Adres, src.Gemeente, src.Gsm, src.Email)))
                .ForMember(
                    dest => dest.Vttl,
                    opts => opts.MapFrom(src => src.ClubIdVttl.HasValue ?
                        PlayerCompetition.Vttl(src.ClubIdVttl.Value, src.ComputerNummerVttl.Value, src.LinkKaartVttl, src.KlassementVttl, src.VolgnummerVttl.Value, src.IndexVttl.Value, src.KlassementWaardeVttl.WaardeVttl) :
                        null))
                //.ForMember(
                //    dest => dest.Vttl,
                //    opts => opts.MapFrom(src => src.ClubIdVttl.HasValue ?
                //        PlayerCompetition.Vttl(src.ClubIdVttl.Value, src.ComputerNummerVttl.Value, src.LinkKaartVttl, src.KlassementVttl, src.VolgnummerVttl.Value, src.IndexVttl.Value, src.KlassementWaardeVttl.WaardeVttl) :
                //        null))
                //.ForMember(
                //    dest => dest.Sporta,
                //    opts => opts.MapFrom(src => src.ClubIdSporta.HasValue ?
                //        PlayerCompetition.Sporta(src.ClubIdSporta.Value, src.LidNummerSporta.Value, src.LinkKaartSporta, src.KlassementSporta, src.VolgnummerSporta.Value, src.IndexSporta.Value, src.KlassementWaardeSporta.WaardeSporta)
                //        : null))
                ;
        }
    }
}