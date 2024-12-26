using AutoMapper;
using Ttc.DataEntities;
using Ttc.Model.Matches;
using Ttc.Model.Teams;

namespace Ttc.DataAccess.Utilities.AutoMapperConfig;

internal class MatchProfile : Profile
{
    public MatchProfile()
    {
        CreateMap<MatchEntity, OtherMatch>()
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
                SetMatchPlayerAliases(match.Players);
                //ChangeMeaningOfHomePlayer(match);
                SetIndividualMatchesOutcome(match.Games, null);
            });

        CreateMap<MatchEntity, Match>()
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
                SetMatchPlayerAliases(match.Players);
                ChangeMeaningOfHomePlayer(match);
                SetIndividualMatchesOutcome(match.Games, match.IsHomeMatch);
            });

        ReportMapping();
    }

    private void ReportMapping()
    {
        CreateMap<MatchCommentEntity, MatchComment>().ReverseMap();
        CreateMap<MatchPlayerEntity, MatchPlayer>().ReverseMap();

        CreateMap<MatchGameEntity, MatchGame>()
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

    private static void SetMatchPlayerAliases(ICollection<MatchPlayer> players)
    {
        foreach (var ply in players)
        {
            ply.Alias = GetFirstName(ply.Name);
        }

        // Fix in case two people are called 'Dirk' etc
        foreach (var ply in players)
        {
            var otherPlayers = players.Where(otherPly => ply.Position != otherPly.Position);
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
    /// to 'is the player a member of TTC Aalst'
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
}
