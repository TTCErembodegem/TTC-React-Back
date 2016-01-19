using System;
using System.Linq.Expressions;
using AutoMapper;
using SimpleInjector;
using Ttc.DataAccess.Entities;
using Ttc.DataAccess.Utilities;
using Ttc.Model;

namespace Ttc.DataAccess
{
    public static class GlobalBackendConfiguration
    {
        public static void Configure()
        {
            ConfigureAutoMapper(new KlassementValueConverter());
        }

        public static void ConfigureIoC(Container container)
        {

        }

        internal static void ConfigureAutoMapper(KlassementValueConverter klassementToValueConverter)
        {
            PlayerMapping(klassementToValueConverter);
            ClubMapping();
        }

        private static void ClubMapping()
        {
        Mapper.CreateMap<ClubEntity, Club>()
                .ForMember(
                    dest => dest.Name,
                    opts => opts.MapFrom(src => src.Naam))
                .ForMember(
                    dest => dest.Active,
                    opts => opts.MapFrom(src => src.Actief))
                .ForMember(
                    dest => dest.Shower,
                    opts => opts.MapFrom(src => src.Douche == 1))
                ;
        }

        #region Automapper Player
        private static void PlayerMapping(KlassementValueConverter klassementToValueConverter)
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
                        CreateVttlPlayer(klassementToValueConverter, src.ClubIdVttl.Value, src.ComputerNummerVttl.Value, src.LinkKaartVttl, src.KlassementVttl, src.VolgnummerVttl.Value, src.IndexVttl.Value)
                        : null))
                .ForMember(
                    dest => dest.Sporta,
                    opts => opts.MapFrom(src => src.ClubIdSporta.HasValue ?
                        CreateSportaPlayer(klassementToValueConverter, src.ClubIdSporta.Value, src.LidNummerSporta.Value, src.LinkKaartSporta, src.KlassementSporta, src.VolgnummerSporta.Value, src.IndexSporta.Value)
                        : null))
                ;
        }

        private static PlayerCompetition CreateSportaPlayer(KlassementValueConverter converter, int clubId, int uniqueIndex, string frenoyLink, string ranking, int position, int rankingIndex)
        {
            return new PlayerCompetition(
                Competition.Sporta,
                clubId, uniqueIndex, frenoyLink, ranking, position, rankingIndex, converter.Sporta(ranking));
        }

        private static PlayerCompetition CreateVttlPlayer(KlassementValueConverter converter, int clubId, int uniqueIndex, string frenoyLink, string ranking, int position, int rankingIndex)
        {
            return new PlayerCompetition(
                Competition.Vttl,
                clubId, uniqueIndex, frenoyLink, ranking, position, rankingIndex, converter.Vttl(ranking));
        }
        #endregion
    }
}