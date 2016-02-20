using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoMapper;
using Ttc.DataEntities;
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
            Mapper.CreateMap<TeamEntity, Team>()
                .ForMember(
                    dest => dest.Competition,
                    opts => opts.MapFrom(src => Constants.NormalizeCompetition(src.Competition)))
                .ForMember(
                    dest => dest.Year,
                    opts => opts.MapFrom(src => src.Year))
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
                    opts => opts.MapFrom(src => src.TeamCode))
                .ForMember(
                    dest => dest.Opponents,
                    opts => opts.MapFrom(src => MapAllTeams(src)))
                ;
        }

        /// <summary>
        /// Map all teams including TTC Erembodegem.
        /// We'll fix this later because multiple TTC Erembodegems could be playing in same team
        /// </summary>
        private static ICollection<OpposingTeam> MapAllTeams(TeamEntity src)
        => src.Opponents.Select(ploeg => new OpposingTeam
        {
            ClubId = ploeg.ClubId,
            TeamCode = ploeg.TeamCode
        }).ToArray();
        #endregion

        #region Matches
        private static void ReportMapping()
        {
            Mapper.CreateMap<MatchPlayerEntity, MatchPlayer>()
                .ReverseMap()
                ;
        
            Mapper.CreateMap<MatchGameEntity, MatchGame>()
                .ForMember(d => d.Outcome, o => o.MapFrom(src => src.WalkOver == WalkOver.None ? MatchOutcome.NotYetPlayed : MatchOutcome.WalkOver))
                .ReverseMap()
                ;
        }

        private static void CalendarMapping()
        {
            Mapper.CreateMap<MatchEntity, Match>()
                .ForMember(
                    dest => dest.TeamId,
                    opts => opts.MapFrom(src => src.HomeTeamId.HasValue ? src.HomeTeamId : src.AwayTeamId))
                .ForMember(
                    dest => dest.Opponent,
                    opts => opts.MapFrom(src => new OpposingTeam
                    {
                        ClubId = src.AwayClubId,
                        TeamCode = src.AwayPloegCode
                    }))
                .ForMember(
                    dest => dest.IsPlayed,
                    opts => opts.MapFrom(src => 
                        GetScoreType(src) != MatchOutcome.NotYetPlayed && 
                        GetScoreType(src) != MatchOutcome.WalkOver && 
                        GetScoreType(src) != MatchOutcome.BeingPlayed))
                .ForMember(
                    dest => dest.Description,
                    opts => opts.MapFrom(src => src.Description))
                .ForMember(
                    dest => dest.ScoreType,
                    opts => opts.MapFrom(src => GetScoreType(src)))
                .ForMember(
                    dest => dest.Score,
                    opts => opts.MapFrom(src => !src.WalkOver || src.HomeScore.HasValue ? new MatchScore(src.HomeScore.Value, src.AwayScore.Value) : null))
                .ForMember(
                    dest => dest.Players,
                    opts => opts.MapFrom(src => src.Players))
                .ForMember(
                    dest => dest.Games,
                    opts => opts.MapFrom(src => src.Games))
                
                .AfterMap((kalender, match) =>
                {
                    SetMatchPlayerAliases(match);
                    ChangeMeaningOfHomePlayer(match);
                    SetIndividualMatchesOutcome(match);
                });
        }

        private static void SetIndividualMatchesOutcome(Match match)
        {
            foreach (var game in match.Games.Where(g => g.Outcome != MatchOutcome.WalkOver))
            {
                game.Outcome = game.HomePlayerSets > game.OutPlayerSets ? MatchOutcome.Won : MatchOutcome.Lost;
                if (!match.IsHomeMatch)
                {
                    game.Outcome = game.Outcome == MatchOutcome.Won ? MatchOutcome.Lost : MatchOutcome.Won;
                }
            }
        }

        private static string GetFirstName(string fullName)
        {
            if (fullName.IndexOf(" ", StringComparison.InvariantCulture) == -1)
            {
                return fullName;
            }
            return fullName.Substring(0, fullName.IndexOf(" ", StringComparison.InvariantCulture));
        }

        private static void SetMatchPlayerAliases(Match match)
        {
            foreach (var ply in match.Players)
            {
                ply.Alias = GetFirstName(ply.Name);
            }

            // Fix in case two people are called 'Dirk' etc
            foreach (var ply in match.Players)
            {
                var otherPlayers = match.Players.Where(otherPly => ply.Position != otherPly.Position);
                if (otherPlayers.Any(otherPly => GetFirstName(otherPly.Alias) == ply.Alias))
                {
                    if (ply.Name.IndexOf(" ", StringComparison.InvariantCulture) != -1)
                    {
                        ply.Alias += ply.Name.Substring(ply.Name.IndexOf(" ", StringComparison.InvariantCulture));
                    }
                }
            }
        }

        private static bool IsOwnClubPlayer(bool isHomeMatch, bool isHomePlayer)
        {
            return (isHomeMatch && isHomePlayer) || (!isHomeMatch && !isHomePlayer);
        }

        /// <summary>
        /// Change the meaning of 'home' from 'was the player playing in his own club'
        /// to 'is the player a member of TTC Erembodegem'
        /// </summary>
        private static void ChangeMeaningOfHomePlayer(Match match)
        {
            // TODO: 'fixing' home might result in derby matches being displayed incorrectly (ex: Sporta A vs Erembodegem A)
            foreach (var ply in match.Players)
            {
                ply.Home = IsOwnClubPlayer(match.IsHomeMatch, ply.Home);
            }
        }

        private static MatchOutcome GetScoreType(MatchEntity kalendar)
        {
            if (kalendar.Date > DateTime.Now || (kalendar.AwayScore == 0 && kalendar.HomeScore == 0))
            {
                if (Constants.HasMatchStarted(kalendar.Date))
                {
                    return MatchOutcome.BeingPlayed;
                }
                return MatchOutcome.NotYetPlayed;
            }
            
            if (kalendar.WalkOver)
            {
                return MatchOutcome.WalkOver;
            }
            if (!kalendar.HomeScore.HasValue || !kalendar.AwayScore.HasValue)
            {
                return MatchOutcome.NotYetPlayed;
            }
            if (kalendar.HomeScore.Value == kalendar.AwayScore.Value && kalendar.HomeScore.Value != 0)
            {
                return MatchOutcome.Draw;
            }

            if (kalendar.IsHomeMatch)
            {
                return kalendar.HomeScore.Value < kalendar.AwayScore.Value ? MatchOutcome.Lost : MatchOutcome.Won;
            }
            else
            {
                return kalendar.HomeScore.Value < kalendar.AwayScore.Value ? MatchOutcome.Won: MatchOutcome.Lost;
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
            Mapper.CreateMap<PlayerEntity, Player>()
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