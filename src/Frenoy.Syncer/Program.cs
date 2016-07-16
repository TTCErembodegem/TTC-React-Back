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
                Configuration.Seed(context, true, true);

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

                context.SaveChanges();
            }
        }
    }
}