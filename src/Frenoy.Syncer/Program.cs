using System;
using System.Collections.Generic;
using System.Linq;
using Frenoy.Api;
using Ttc.DataAccess;
using Ttc.DataAccess.Migrations;
using Ttc.Model.Players;

namespace Frenoy.Syncer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var context = new TtcDbContext())
            {
                try
                {
                    //var vttlPlayers = new FrenoyPlayersApi(context, Competition.Vttl);
                    //vttlPlayers.StopAllPlayers();
                    //vttlPlayers.SyncPlayers();
                    var sportaPlayers = new FrenoyPlayersApi(context, Competition.Sporta);
                    sportaPlayers.SyncPlayers();


                    //Configuration.Seed(context, false);

                    //var vttl = new FrenoyMatchesApi(context, Competition.Vttl);
                    //vttl.SyncTeamsAndMatches();
                    //var sporta = new FrenoyMatchesApi(context, Competition.Sporta);
                    //sporta.SyncTeamsAndMatches();

                    //ChangeMatchDates(context);

                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Console.ReadKey();
                }
            }
        }

        #region End of season tampering
        /// <summary>
        /// If there is no real life data between seasons,
        /// change some match dates to around now for testing purposes
        /// </summary>
        private void ChangeMatchDates(TtcDbContext context)
        {
            bool endOfSeason = !context.Matches.Any(match => match.Date > DateTime.Now);
            if (endOfSeason)
            {
                var passedMatches = context.Matches
                    .Where(x => x.Date < DateTime.Today)
                    .OrderByDescending(x => x.Date)
                    .Take(42);

                var timeToAdd = DateTime.Today - passedMatches.First().Date;
                foreach (var match in passedMatches.Take(20))
                {
                    match.Date = match.Date.Add(timeToAdd);
                }

                var rnd = new Random();
                foreach (var match in passedMatches.Take(20))
                {
                    match.Date = DateTime.Today.Add(TimeSpan.FromDays(rnd.Next(1, 20))).AddHours(rnd.Next(10, 20));
                    match.Description = "";
                    match.AwayScore = null;
                    match.HomeScore = null;
                    //match.IsSyncedWithFrenoy = true;
                    match.WalkOver = false;

                    context.MatchComments.RemoveRange(match.Comments.ToArray());
                    context.MatchGames.RemoveRange(match.Games.ToArray());
                    context.MatchPlayers.RemoveRange(match.Players.ToArray());
                }
            }
        }
        #endregion
    }
}