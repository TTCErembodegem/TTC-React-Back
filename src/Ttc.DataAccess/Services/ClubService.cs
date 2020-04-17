using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Ttc.DataEntities;
using System.Data.Entity;
using System.Threading.Tasks;
using Ttc.Model.Clubs;
using Ttc.Model.Players;

namespace Ttc.DataAccess.Services
{
    public class ClubService : BaseService
    {
        private static IEnumerable<Club> _clubs;

        public async Task<IEnumerable<Club>> GetActiveClubs()
        {
            if (MatchService.MatchesPlaying && _clubs != null)
            {
                return _clubs;
            }

            using (var dbContext = new TtcDbContext())
            {
                var activeClubs = await dbContext.Clubs
                    .Include(x => x.Lokalen)
                    .Include(x => x.Contacten)
                    .Where(x => x.Actief.HasValue && x.Actief == 1)
                    .ToListAsync();

                var result = Mapper.Map<IList<ClubEntity>, IList<Club>>(activeClubs);

                var managers = activeClubs.Single(x => x.Id == Constants.OwnClubId).Contacten;
                var managerIds = managers.Select(x => x.SpelerId).ToArray();
                

                var ourClub = result.Single(x => x.Id == Constants.OwnClubId);
                ourClub.Managers = new List<ClubManager>();

                var managerPlayers = await dbContext.Players.Where(x => managerIds.Contains(x.Id)).ToArrayAsync();
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

                if (MatchService.MatchesPlaying)
                {
                    _clubs = result;
                }

                return result;
            }
        }

        #region Club Board
        public async Task SaveBoardMember(int playerId, string boardFunction, int sort)
        {
            using (var context = new TtcDbContext())
            {
                var board = await context.ClubContacten.SingleOrDefaultAsync(x => x.SpelerId == playerId);
                if (board == null)
                {
                    board = new ClubContact()
                    {
                        ClubId = Constants.OwnClubId,
                        SpelerId = playerId
                    };
                    context.ClubContacten.Add(board);
                }

                board.Omschrijving = boardFunction;
                board.Sortering = sort;
                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteBoardMember(int playerId)
        {
            using (var context = new TtcDbContext())
            {
                var board = await context.ClubContacten.SingleAsync(x => x.SpelerId == playerId);
                context.ClubContacten.Remove(board);
                await context.SaveChangesAsync();
            }
        }
        #endregion

        public async Task<Club> UpdateClub(Club club)
        {
            using (var dbContext = new TtcDbContext())
            {
                var existingClub = await dbContext.Clubs.FirstOrDefaultAsync(x => x.Id == club.Id);
                if (existingClub == null)
                {
                    throw new Exception("Club not found");
                }
                
                MapClub(club, existingClub);
                await dbContext.SaveChangesAsync();
            }
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
}