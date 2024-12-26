using AutoMapper;
using Ttc.DataEntities;
using Microsoft.EntityFrameworkCore;
using Ttc.DataEntities.Core;
using Ttc.Model.Clubs;
using Ttc.Model.Players;

namespace Ttc.DataAccess.Services;

public class ClubService
{
    private readonly ITtcDbContext _context;
    private readonly IMapper _mapper;

    public ClubService(ITtcDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<Club>> GetActiveClubs()
    {
        var activeClubs = await _context.Clubs
            .Include(x => x.Lokalen)
            .Include(x => x.Contacten)
            .Where(x => x.Actief == 1)
            .ToListAsync();

        var result = _mapper.Map<IList<ClubEntity>, IList<Club>>(activeClubs);

        var managers = activeClubs.Single(x => x.Id == Constants.OwnClubId).Contacten;
        var managerIds = managers.Select(x => x.SpelerId).ToArray();


        var ourClub = result.Single(x => x.Id == Constants.OwnClubId);
        ourClub.Managers = new List<ClubManager>();

        var managerPlayers = await _context.Players.Where(x => managerIds.Contains(x.Id)).ToArrayAsync();
        foreach (var managerPlayer in managerPlayers)
        {
            var managerInfo = managers.Single(x => x.SpelerId == managerPlayer.Id);
            ourClub.Managers.Add(new ClubManager
            {
                Description = managerInfo.Omschrijving,
                PlayerId = managerInfo.SpelerId,
                Name = managerPlayer.Name,
                Contact = new PlayerContact(managerPlayer.Id, managerPlayer.Email, managerPlayer.Gsm, managerPlayer.Adres, managerPlayer.Gemeente),
                SortOrder = managerInfo.Sortering
            });
        }

        return result;
    }

    #region Club Board
    public async Task SaveBoardMember(int playerId, string boardFunction, int sort)
    {
        var board = await _context.ClubContacten.SingleOrDefaultAsync(x => x.SpelerId == playerId);
        if (board == null)
        {
            board = new ClubContact()
            {
                ClubId = Constants.OwnClubId,
                SpelerId = playerId
            };
            _context.ClubContacten.Add(board);
        }

        board.Omschrijving = boardFunction;
        board.Sortering = sort;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteBoardMember(int playerId)
    {
        var board = await _context.ClubContacten.SingleAsync(x => x.SpelerId == playerId);
        _context.ClubContacten.Remove(board);
        await _context.SaveChangesAsync();
    }
    #endregion

    public async Task<Club> UpdateClub(Club club)
    {
        var existingClub = await _context.Clubs.FirstOrDefaultAsync(x => x.Id == club.Id);
        if (existingClub == null)
        {
            throw new Exception("Club not found");
        }

        MapClub(club, existingClub);
        await _context.SaveChangesAsync();
        return club;
    }

    private static void MapClub(Club club, ClubEntity existingClub)
    {
        existingClub.Naam = club.Name;
        existingClub.Douche = club.Shower ? 1 : 0;
        existingClub.Website = club.Website;
        // existingClub.Lokalen = club.MainLocation
    }
}
