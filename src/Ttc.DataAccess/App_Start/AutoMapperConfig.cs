using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoMapper;
using SimpleInjector.Advanced.Internal;
using Ttc.DataAccess.Entities;
using Ttc.DataAccess.Utilities;
using Ttc.Model;
using Ttc.Model.Clubs;
using Ttc.Model.Matches;
using Ttc.Model.Players;
using Ttc.Model.Teams;

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
            ReportMapping();
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
                    dest => dest.Id,
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
        private static void ReportMapping()
        {
            Mapper.CreateMap<VerslagSpeler, MatchPlayer>()
                .ForMember(
                    dest => dest.Home,
                    opts => opts.MapFrom(src => src.Thuis == 1))
                .ForMember(
                    dest => dest.Name,
                    opts => opts.MapFrom(src => src.SpelerNaam))
                .ForMember(
                    dest => dest.Position,
                    opts => opts.MapFrom(src => src.Positie))
                .ForMember(
                    dest => dest.MatchId,
                    opts => opts.MapFrom(src => src.KalenderId))
                .ForMember(
                    dest => dest.Ranking,
                    opts => opts.MapFrom(src => src.Klassement))
                .ForMember(
                    dest => dest.UniqueIndex,
                    opts => opts.MapFrom(src => src.UniqueIndex))
                .ForMember(
                    dest => dest.Won,
                    opts => opts.MapFrom(src => src.Winst))
                .ForMember(
                    dest => dest.PlayerId,
                    opts => opts.MapFrom(src => src.SpelerId == 0 ? (int?)null : src.SpelerId))
                .ReverseMap()
                ;
        
            Mapper.CreateMap<VerslagIndividueel, MatchGame>()
                .ForMember(
                    dest => dest.MatchId,
                    opts => opts.MapFrom(src => src.KalenderId))
                .ForMember(
                    dest => dest.MatchNumber,
                    opts => opts.MapFrom(src => src.MatchNummer))
                .ForMember(
                    dest => dest.HomePlayerUniqueIndex,
                    opts => opts.MapFrom(src => src.ThuisSpelerUniqueIndex))
                .ForMember(
                    dest => dest.HomePlayerSets,
                    opts => opts.MapFrom(src => src.ThuisSpelerSets))
                .ForMember(
                    dest => dest.OutPlayerUniqueIndex,
                    opts => opts.MapFrom(src => src.UitSpelerUniqueIndex))
                .ForMember(
                    dest => dest.OutPlayerSets,
                    opts => opts.MapFrom(src => src.UitSpelerSets))
                ;
        }

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
                    dest => dest.TeamId,
                    opts => opts.MapFrom(src => src.ThuisClubPloeg.ReeksId))

                .ForMember(
                    dest => dest.TeamId,
                    opts => opts.MapFrom(src => src.ThuisClubPloeg.ReeksId))

                .ForMember(
                    dest => dest.Opponent,
                    opts => opts.MapFrom(src => new OpposingTeam
                    {
                        ClubId = src.UitClubId.Value,
                        TeamCode = src.UitPloeg
                    }))
                .ForMember(
                    dest => dest.IsPlayed,
                    opts => opts.MapFrom(src => GetScoreType(src.Verslag) != MatchOutcome.NotYetPlayed && GetScoreType(src.Verslag) != MatchOutcome.WalkOver))
                .ForMember(
                    dest => dest.Description,
                    opts => opts.MapFrom(src => src.Beschrijving))
                .ForMember(
                    dest => dest.ReportPlayerId,
                    opts => opts.MapFrom(src => src.Verslag.SpelerId))
                .ForMember(
                    dest => dest.ScoreType,
                    opts => opts.MapFrom(src => GetScoreType(src.Verslag)))
                .ForMember(
                    dest => dest.Score,
                    opts => opts.MapFrom(src => src.Verslag.WO == 0 || src.Verslag.UitslagThuis.HasValue ? new MatchScore(src.Verslag.UitslagThuis.Value, src.Verslag.UitslagUit.Value) : null))
                .ForMember(
                    dest => dest.Players,
                    opts => opts.MapFrom(src => src.Verslag.Spelers))
                .ForMember(
                    dest => dest.Games,
                    opts => opts.MapFrom(src => src.Verslag.Individueel))
                ;
        }

        private static MatchOutcome GetScoreType(Verslag verslag)
        {
            if (verslag == null)
            {
                return MatchOutcome.NotYetPlayed;
            }
            if (verslag.WO == 1)
            {
                return MatchOutcome.WalkOver;
            }
            if (!verslag.UitslagThuis.HasValue || !verslag.UitslagUit.HasValue)
            {
                return MatchOutcome.NotYetPlayed;
            }
            if (verslag.UitslagThuis.Value == verslag.UitslagUit.Value)
            {
                return MatchOutcome.Draw;
            }

            if (verslag.Kalender.Thuis.Value == 1)
            {
                return verslag.UitslagThuis.Value < verslag.UitslagUit.Value ? MatchOutcome.Lost : MatchOutcome.Won;
            }
            else
            {
                return verslag.UitslagThuis.Value < verslag.UitslagUit.Value ? MatchOutcome.Won: MatchOutcome.Lost;
            }
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
                PlayerId = x.SpelerId,
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
                return new ClubLocation();
            }
            return CreateClubLocation(mainLocation);
        }

        private static ClubLocation CreateClubLocation(ClubLokaal location)
        =>  new ClubLocation
            {
                Id = location.Id,
                Description = location.Lokaal,
                Address = location.Adres,
                PostalCode = location.Postcode.ToString(),
                City = location.Gemeente,
                Mobile = location.Telefoon
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
                    dest => dest.Active,
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