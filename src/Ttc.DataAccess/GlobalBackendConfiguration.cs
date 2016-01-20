using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using SimpleInjector;
using Ttc.DataAccess.Entities;
using Ttc.DataAccess.Utilities;
using Ttc.Model;
using Ttc.Model.Clubs;
using Ttc.Model.Divisions;
using Ttc.Model.Players;

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
            CalendarMapping();
            DivisionMapping();
        }

        private static void DivisionMapping()
        {
            Mapper.CreateMap<Reeks, Division>()
                .ForMember(
                    dest => dest.Competition,
                    opts => opts.MapFrom(src => Constants.NormalizeCompetition(src.Competitie)))
                .ForMember(
                    dest => dest.Year,
                    opts => opts.MapFrom(src => src.Jaar.Value))
                .ForMember(
                    dest => dest.DivisionName,
                    opts => opts.MapFrom(src => src.ReeksNummer + src.ReeksCode))
                .ForMember(
                    dest => dest.Frenoy,
                    opts => opts.MapFrom(src => new FrenoyTeamLinks
                    {
                        DivisionId = src.FrenoyDivisionId,
                        LinkId = src.LinkId,
                        TeamId = src.FrenoyTeamId
                    }))
                .ForMember(
                    dest => dest.TeamCode,
                    opts => opts.MapFrom(src => FindOwnTeamCode(src)))
                .ForMember(
                    dest => dest.Opponents,
                    opts => opts.MapFrom(src => MapAllTeams(src)))
                ;
        }

        /// <summary>
        /// Map all teams including TTC Erembodegem.
        /// We'll fix this later because multiple TTC Erembodegems could be playing in same Reeks
        /// </summary>
        private static ICollection<OpposingTeams> MapAllTeams(Reeks src)
        {
            return src.Ploegen.Select(ploeg => new OpposingTeams
            {
                ClubId = ploeg.ClubId.Value,
                TeamCode = ploeg.Code
            }).ToArray();
        }

        /// <summary>
        /// Incorrect when multiple own club teams in Reeks
        /// </summary>
        private static string FindOwnTeamCode(Reeks src)
        {
            return src.Ploegen.First(x => x.ClubId == Constants.OwnClubId).Code;
        }

        #region CalendarItems
        private static void CalendarMapping()
        {
            Mapper.CreateMap<Kalender, Match>()
                .ForMember(
                    dest => dest.Date,
                    opts => opts.MapFrom(src => src.Datum + src.Uur))
               .ForMember(
                    dest => dest.IsHomeMatch,
                    opts => opts.MapFrom(src => src.Thuis.HasValue && src.Thuis == 1))
            ;
        }
        #endregion

        #region Clubs
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
                .ForMember(
                    dest => dest.Managers,
                    opts => opts.MapFrom(src => CreateClubManagers(src.Contacten)))
                .ForMember(
                    dest => dest.MainLocation,
                    opts => opts.MapFrom(src => CreateMainClubLocation(src.Lokalen)))
                .ForMember(
                    dest => dest.AlternativeLocations,
                    opts => opts.MapFrom(src => CreateSecundaryClubLocations(src.Lokalen)));
        }

        private static ICollection<ClubManager> CreateClubManagers(ICollection<ClubContact> contacten)
        {
            if (!contacten.Any())
            {
                return new Collection<ClubManager>();
            }
            return contacten.OrderBy(x => x.Sortering).Select(x => new ClubManager
            {
                SpelerId = x.SpelerId,
                Description = x.Omschrijving
            }).ToList();
        }

        private static ICollection<ClubLocation> CreateSecundaryClubLocations(ICollection<ClubLokaal> lokalen)
        {
            var locations = lokalen.Where(x => !x.Hoofd.HasValue || x.Hoofd != 1).ToArray();
            if (!locations.Any())
            {
                return new Collection<ClubLocation>();
            }
            return locations.Select(CreateClubLocation).ToArray();
        }

        private static ClubLocation CreateMainClubLocation(ICollection<ClubLokaal> lokalen)
        {
            var mainLocation = lokalen.FirstOrDefault(x => x.Hoofd.HasValue && x.Hoofd == 1);
            if (mainLocation == null)
            {
                return null;
            }
            return CreateClubLocation(mainLocation);
        }

        private static ClubLocation CreateClubLocation(ClubLokaal location)
        {
            return new ClubLocation
            {
                Id = location.Id,
                Description = location.Lokaal,
                Contact = new Contact
                {
                    Address = location.Adres,
                    City = $"{location.Postcode} {location.Gemeente}",
                    Email = null,
                    Mobile = location.Telefoon
                }
            };
        }
        #endregion

        #region Players
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