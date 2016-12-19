using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Ttc.DataEntities;
using System.Data.Entity;
using Ttc.Model.Clubs;
using Ttc.Model.Players;

namespace Ttc.DataAccess.Services
{
    public class ClubService : BaseService
    {
        private static IEnumerable<Club> _clubs;

        public IEnumerable<Club> GetActiveClubs()
        {
            if (MatchService.MatchesPlaying && _clubs != null)
            {
                return _clubs;
            }

            using (var dbContext = new TtcDbContext())
            {
                var activeClubs = dbContext.Clubs
                    .Include(x => x.Lokalen)
                    .Include(x => x.Contacten)
                    .Where(x => x.Actief.HasValue && x.Actief == 1)
                    .ToList();

                var result = Mapper.Map<IList<ClubEntity>, IList<Club>>(activeClubs);

                var managers = activeClubs.Single(x => x.Id == Constants.OwnClubId).Contacten;
                var managerIds = managers.Select(x => x.SpelerId).ToArray();
                

                var ourClub = result.Single(x => x.Id == Constants.OwnClubId);
                ourClub.Managers = new List<ClubManager>();

                var managerPlayers = dbContext.Players.Where(x => managerIds.Contains(x.Id));
                foreach (var managerPlayer in managerPlayers)
                {
                    var managerInfo = managers.Single(x => x.SpelerId == managerPlayer.Id);
                    ourClub.Managers.Add(new ClubManager
                    {
                        Description = string.IsNullOrWhiteSpace(managerInfo.Omschrijving) ? ClubManagerType.Default : (ClubManagerType)Enum.Parse(typeof(ClubManagerType), managerInfo.Omschrijving, true),
                        PlayerId = managerInfo.SpelerId,
                        Name = managerPlayer.Naam,
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
    }
}