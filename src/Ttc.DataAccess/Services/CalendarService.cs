using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using AutoMapper;
using Ttc.DataAccess.Entities;
using Ttc.Model.Matches;

namespace Ttc.DataAccess.Services
{
    public class CalendarService
    {
        public IEnumerable<Match> GetRelevantMatches()
        {
            using (var dbContext = new TtcDbContext())
            {
                var dateBegin = DateTime.Now.AddDays(-12);
                var dateEnd = DateTime.Now.AddDays(2);

                var calendar = dbContext.Kalender
                    .Include(x => x.ThuisClubPloeg)
                    .Include(x => x.Verslag)
                    .Include("Verslag.Individueel")
                    .Include("Verslag.Spelers")
                    //.Where(x => x.Id == 1597)
                    .Where(x => x.Datum >= dateBegin)
                    .Where(x => x.Datum <= dateEnd)
                    .Where(x => x.ThuisClubId.HasValue)
                    .OrderBy(x => x.Datum)
                    .ToList();

                var result = Mapper.Map<IList<Kalender>, IList<Match>>(calendar);
                foreach (var match in result)
                {
                    FixUp(match);
                }
                
                return result;
            }
        }

        #region Fixing some stuff from the db
        private static bool IsOwnClubPlayer(bool isHomeMatch, bool isHomePlayer)
        {
            return (isHomeMatch && isHomePlayer) || (!isHomeMatch && !isHomePlayer);
        }

        private static string GetFirstName(string fullName)
        {
            if (fullName.IndexOf(" ", StringComparison.InvariantCulture) == -1)
            {
                return fullName;
            }
            return fullName.Substring(0, fullName.IndexOf(" ", StringComparison.InvariantCulture));
        }

        private static void FixUp(Match match)
        {
            // TODO: 'fixing' home might result in derby matches being displayed incorrectly (ex: Sporta A vs Erembodegem A)
            // Fix in case two people are called 'Dirk' etc
            foreach (var ply in match.Players)
            {
                ply.Alias = GetFirstName(ply.Name);
            }
            foreach (var ply in match.Players)
            {
                var otherPlayers = match.Players.Where(otherPly => ply.Position != otherPly.Position);
                if (otherPlayers.Any(otherPly => GetFirstName(otherPly.Alias) == ply.Alias))
                {
                    ply.Alias += ply.Name.Substring(ply.Name.IndexOf(" ", StringComparison.InvariantCulture));
                }
            }

            // Change the meaning of 'home' from 'was the player playing in his own club'
            // to 'is the player a member of TTC Erembodegem'
            foreach (var ply in match.Players)
            {
                ply.Home = IsOwnClubPlayer(match.IsHomeMatch, ply.Home);
            }
        }
        #endregion

        public Match GetMatch(int matchId)
        {
            using (var dbContext = new TtcDbContext())
            {
                return Mapper.Map<Kalender, Match>(dbContext.Kalender.SingleOrDefault(x => x.Id == matchId));
            }
        }

        public void DeleteMatchPlayer(MatchPlayer matchPlayer)
        {
        
        }

        public MatchPlayer AddMatchPlayer(MatchPlayer matchPlayer)
        {
            using (var dbContext = new TtcDbContext())
            {
                var verslagSpeler = Mapper.Map<MatchPlayer, VerslagSpeler>(matchPlayer);
                //dbContext.VerslagenSpelers.Add(verslagSpeler);
                dbContext.SaveChanges();
            }
            return matchPlayer;
        }
    }
}