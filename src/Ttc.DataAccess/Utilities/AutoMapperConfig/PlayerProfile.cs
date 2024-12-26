using AutoMapper;
using Ttc.DataEntities;
using Ttc.Model.Players;

namespace Ttc.DataAccess.Utilities.AutoMapperConfig;

internal class PlayerProfile : Profile
{
    public PlayerProfile()
    {
        CreateMap<PlayerEntity, Player>()
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
                    CreateVttlPlayer(src.ClubIdVttl.Value, src.ComputerNummerVttl.Value, src.LinkKaartVttl, src.KlassementVttl, src.VolgnummerVttl.Value, src.IndexVttl.Value, src.NextKlassementVttl)
                    : null))
            .ForMember(
                dest => dest.Sporta,
                opts => opts.MapFrom(src => src.ClubIdSporta.HasValue ?
                    CreateSportaPlayer(src.ClubIdSporta.Value, src.LidNummerSporta.Value, src.LinkKaartSporta, src.KlassementSporta, src.VolgnummerSporta.Value, src.IndexSporta.Value, src.NextKlassementSporta)
                    : null))
            ;
    }

    private static PlayerCompetition CreateSportaPlayer(int clubId, int uniqueIndex, string frenoyLink, string ranking, int position, int rankingIndex, string prevRanking)
    {
        return new PlayerCompetition(
            Competition.Sporta,
            clubId, uniqueIndex, frenoyLink, ranking, position, rankingIndex, KlassementValueConverter.Sporta(ranking), prevRanking);
    }

    private static PlayerCompetition CreateVttlPlayer(int clubId, int uniqueIndex, string frenoyLink, string ranking, int position, int rankingIndex, string prevRanking)
    {
        return new PlayerCompetition(
            Competition.Vttl,
            clubId, uniqueIndex, frenoyLink, ranking, position, rankingIndex, KlassementValueConverter.Vttl(ranking), prevRanking);
    }
}
