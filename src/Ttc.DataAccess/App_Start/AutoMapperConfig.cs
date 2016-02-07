using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoMapper;
using Ttc.DataAccess.Entities;
using Ttc.DataAccess.Utilities;
using Ttc.Model;
using Ttc.Model.Clubs;
using Ttc.Model.Players;
using Ttc.Model.Teams;
using Ttc.Model.Matches;

namespace Ttc.DataAccess.App_Start
{
    internal static class AutoMapperConfig
    {
        public static void Configure(KlassementValueConverter klassementToValueConverter)
        {
            PlayerMapping(klassementToValueConverter);
            ClubMapping();
            CalendarMapping();
            TeamMapping();
        }

        #region Teams
        private static void TeamMapping()
        {
            Mapper.CreateMap<Reeks, Team>()
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
                    dest => dest.ReeksId,
                    opts => opts.MapFrom(src => src.Id))
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
        private static ICollection<OpposingTeam> MapAllTeams(Reeks src)
        =>  src.Ploegen.Select(ploeg => new OpposingTeam
            {
                ClubId = ploeg.ClubId.Value,
                TeamCode = ploeg.Code
            }).ToArray();

        /// <summary>
        /// Incorrect when multiple own club teams in Reeks
        /// </summary>
        private static string FindOwnTeamCode(Reeks src) => src.Ploegen.First(x => x.ClubId == Constants.OwnClubId).Code;

        #endregion

        #region Matches
        private static void CalendarMapping()
        {
            Mapper.CreateMap<Kalender, Match>()
                .ForMember(
                    dest => dest.Date,
                    opts => opts.MapFrom(src => src.Datum + src.Uur))
                .ForMember(
                    dest => dest.IsHomeMatch,
                    opts => opts.MapFrom(src => src.Thuis.HasValue && src.Thuis == 1))
                .ForMember(
                    dest => dest.ReeksId,
                    opts => opts.MapFrom(src => src.ThuisClubPloeg.ReeksId))
                .ForMember(
                    dest => dest.Opponent,
                    opts => opts.MapFrom(src => new OpposingTeam
                    {
                        ClubId = src.UitClubId.Value,
                        TeamCode = src.UitPloeg
                    }))
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
        =>  new ClubLocation
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
            => new PlayerCompetition(
                Competition.Sporta,
                clubId, uniqueIndex, frenoyLink, ranking, position, rankingIndex, converter.Sporta(ranking));

        private static PlayerCompetition CreateVttlPlayer(KlassementValueConverter converter, int clubId, int uniqueIndex, string frenoyLink, string ranking, int position, int rankingIndex)
            => new PlayerCompetition(
                Competition.Vttl,
                clubId, uniqueIndex, frenoyLink, ranking, position, rankingIndex, converter.Vttl(ranking));

        #endregion
    }
}