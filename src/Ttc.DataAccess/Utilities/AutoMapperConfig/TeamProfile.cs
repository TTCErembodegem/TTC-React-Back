using AutoMapper;
using Ttc.DataEntities;
using Ttc.Model.Teams;

namespace Ttc.DataAccess.Utilities.AutoMapperConfig;

internal class TeamProfile : Profile
{
    public TeamProfile()
    {
        CreateMap<TeamPlayerEntity, TeamPlayer>()
            .ForMember(
                dest => dest.Type,
                opts => opts.MapFrom(src => src.PlayerType));

        CreateMap<TeamEntity, Team>()
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
}
