using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoMapper;
using Ttc.DataAccess.Utilities;
using Ttc.DataEntities;
using Ttc.Model.Clubs;
using Ttc.Model.Matches;
using Ttc.Model.Players;
using Ttc.Model.Teams;

namespace Ttc.DataAccess
{
    internal static class AutoMapperConfig
    {
        internal static MapperConfiguration Factory;

        public static void Configure(KlassementValueConverter klassementToValueConverter)
        {
            Factory = new MapperConfiguration(cfg =>
            {
                PlayerMapping(cfg, klassementToValueConverter);
                ClubMapping(cfg);
                MatchMapping(cfg);
                TeamMapping(cfg);
                ReportMapping(cfg);
            });
        }

        #region Teams
        private static void TeamMapping(IMapperConfiguration cfg)
        {
            cfg.CreateMap<TeamPlayerEntity, TeamPlayer>()
                .ForMember(
                    dest => dest.Type,
                    opts => opts.MapFrom(src => src.PlayerType));

            cfg.CreateMap<TeamEntity, Team>()
            .ForMember(
                dest => dest.ClubId,
                opts => opts.MapFrom(src => Constants.OwnClubId))
            .ForMember(
                dest => dest.Competition,
                opts => opts.MapFrom(src => Constants.NormalizeCompetition(src.Competition)))
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
                dest => dest.Players,
                opts => opts.MapFrom(src => src.Players))
            .ForMember(
                dest => dest.Opponents,
                opts => opts.MapFrom(src => MapAllTeams(src)))
            ;
        }

        private static ICollection<OpposingTeam> MapAllTeams(TeamEntity src)
        {
            return src.Opponents.Select(opponent => new OpposingTeam
            {
                ClubId = opponent.ClubId,
                TeamCode = opponent.TeamCode
            }).ToArray();
        }

        #endregion

        #region Matches
        private static void ReportMapping(IMapperConfiguration cfg)
        {
            cfg.CreateMap<MatchCommentEntity, MatchComment>().ReverseMap();
            cfg.CreateMap<MatchPlayerEntity, MatchPlayer>().ReverseMap();

            cfg.CreateMap<MatchGameEntity, MatchGame>()
                .ForMember(d => d.Outcome, o => o.MapFrom(src => src.WalkOver == WalkOver.None ? MatchOutcome.NotYetPlayed : MatchOutcome.WalkOver))
                .ForMember(
                    dest => dest.OutPlayerSets,
                    opts => opts.MapFrom(src => src.AwayPlayerSets))
                .ForMember(
                    dest => dest.OutPlayerUniqueIndex,
                    opts => opts.MapFrom(src => src.AwayPlayerUniqueIndex))
                .ReverseMap()
                ;
        }

        private static void MatchMapping(IMapperConfiguration cfg)
        {
            cfg.CreateMap<MatchEntity, OtherMatch>()
                .ForMember(
                    dest => dest.Home,
                    opts => opts.MapFrom(src => new OpposingTeam
                    {
                        ClubId = src.HomeClubId,
                        TeamCode = src.HomeTeamCode
                    }))
                .ForMember(
                    dest => dest.Away,
                    opts => opts.MapFrom(src => new OpposingTeam
                    {
                        ClubId = src.AwayClubId,
                        TeamCode = src.AwayTeamCode
                    }))
                .ForMember(
                    dest => dest.IsPlayed,
                    opts => opts.MapFrom(src =>
                        GetScoreType(src) != MatchOutcome.NotYetPlayed &&
                        GetScoreType(src) != MatchOutcome.BeingPlayed))
                .ForMember(
                    dest => dest.ScoreType,
                    opts => opts.MapFrom(src => GetScoreType(src)))
                .ForMember(
                    dest => dest.Score,
                    opts => opts.MapFrom(src => !src.WalkOver && src.HomeScore.HasValue ? new MatchScore(src.HomeScore.Value, src.AwayScore.Value) : null))

                .AfterMap((matchEntity, match) =>
                {
                    SetMatchPlayerAliases(match);
                    //ChangeMeaningOfHomePlayer(match);
                    SetIndividualMatchesOutcome(match.Games, null);
                });

            cfg.CreateMap<MatchEntity, Match>()
                .ForMember(
                    dest => dest.TeamId,
                    opts => opts.MapFrom(src => src.HomeTeamId.HasValue ? src.HomeTeamId : src.AwayTeamId))
                .ForMember(
                    dest => dest.Opponent,
                    opts => opts.MapFrom(src => new OpposingTeam
                    {
                        ClubId = src.HomeTeamId.HasValue ? src.AwayClubId : src.HomeClubId,
                        TeamCode = src.HomeTeamId.HasValue ? src.AwayTeamCode : src.HomeTeamCode
                    }))
                .ForMember(
                    dest => dest.IsPlayed,
                    opts => opts.MapFrom(src =>
                        GetScoreType(src) != MatchOutcome.NotYetPlayed &&
                        GetScoreType(src) != MatchOutcome.BeingPlayed))
                .ForMember(
                    dest => dest.ScoreType,
                    opts => opts.MapFrom(src => GetScoreType(src)))
                .ForMember(
                    dest => dest.Score,
                    opts => opts.MapFrom(src => !src.WalkOver && src.HomeScore.HasValue ? new MatchScore(src.HomeScore.Value, src.AwayScore.Value) : null))

                .AfterMap((matchEntity, match) =>
                {
                    SetMatchPlayerAliases(match);
                    ChangeMeaningOfHomePlayer(match);
                    SetIndividualMatchesOutcome(match.Games, match.IsHomeMatch);
                });
        }

        private static void SetIndividualMatchesOutcome(IEnumerable<MatchGame> games, bool? isHomeMatch)
        {
            foreach (var game in games.Where(g => g.Outcome != MatchOutcome.WalkOver))
            {
                game.Outcome = game.HomePlayerSets > game.OutPlayerSets ? MatchOutcome.Won : MatchOutcome.Lost;
                if (isHomeMatch.HasValue && !isHomeMatch.Value)
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

        private static void SetMatchPlayerAliases(IMatch match)
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
            if (match.IsHomeMatch.HasValue)
            {
                foreach (var ply in match.Players)
                {
                    ply.Home = IsOwnClubPlayer(match.IsHomeMatch.Value, ply.Home);
                }
            }
        }

        private static MatchOutcome GetScoreType(MatchEntity match)
        {
            var now = DateTime.Now;
            var yesterday = now.Subtract(TimeSpan.FromDays(1));
            if ((match.Date.Date == now.Date && now.Hour >= match.Date.Hour - 10) || (match.Date.Date == yesterday.Date && now.Hour < match.Date.Hour - 10))
            {
                return MatchOutcome.BeingPlayed;
            }
            if (match.Date.Date >= DateTime.Now.Date)
            {
                return MatchOutcome.NotYetPlayed;
            }

            if (match.WalkOver)
            {
                return MatchOutcome.WalkOver;
            }
            if (!match.HomeScore.HasValue || !match.AwayScore.HasValue)
            {
                return MatchOutcome.NotYetPlayed;
            }
            if (match.HomeScore.Value == match.AwayScore.Value)
            {
                return MatchOutcome.Draw;
            }

            if (match.IsHomeMatch.HasValue && match.IsHomeMatch.Value)
            {
                return match.HomeScore.Value < match.AwayScore.Value ? MatchOutcome.Lost : MatchOutcome.Won;
            }
            else
            {
                return match.HomeScore.Value < match.AwayScore.Value ? MatchOutcome.Won : MatchOutcome.Lost;
            }
        }
        #endregion

        #region Clubs
        private static void ClubMapping(IMapperConfiguration cfg)
        {
            cfg.CreateMap<ClubEntity, Club>()
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
                    dest => dest.MainLocation,
                    opts => opts.MapFrom(src => CreateMainClubLocation(src.Lokalen)))
                .ForMember(
                    dest => dest.AlternativeLocations,
                    opts => opts.MapFrom(src => CreateSecundaryClubLocations(src.Lokalen)));
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
        {
            return new ClubLocation
            {
                Id = location.Id,
                Description = location.Lokaal,
                Address = location.Adres,
                PostalCode = location.Postcode.ToString(),
                City = location.Gemeente,
                Mobile = location.Telefoon
            };
        }

        #endregion

        #region Players
        private static void PlayerMapping(IMapperConfiguration cfg, KlassementValueConverter klassementToValueConverter)
        {
            cfg.CreateMap<PlayerEntity, Player>()
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
                    dest => dest.QuitYear,
                    opts => opts.MapFrom(src => src.Gestopt))
                .ForMember(
                    dest => dest.Security,
                    opts => opts.MapFrom(src => src.Toegang.ToString()))
                .ForMember(
                    dest => dest.Style,
                    opts => opts.MapFrom(src => new PlayerStyle(src.Id, src.Stijl, src.BesteSlag)))
                .ForMember(
                    dest => dest.Contact,
                    opts => opts.MapFrom(src => new PlayerContact(src.Id, src.Email, src.Gsm, src.Adres, src.Gemeente)))
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