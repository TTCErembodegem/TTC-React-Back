using System.Collections.ObjectModel;
using AutoMapper;
using Ttc.DataEntities;
using Ttc.Model.Clubs;

namespace Ttc.DataAccess.Utilities.AutoMapperConfig;

internal class ClubProfile : Profile
{
    public ClubProfile()
    {
        CreateMap<ClubEntity, Club>()
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
}
