# TTC-React-Back

TODO stuff:
- Implement caching again...
- Broadcasting with SignalR?
- TabbedContainer: Accordion isn't implemented
- Check all commits done after starting the switch to TypeScript
- Check the old Ttc.UnitTests project -- anything interesting there?

- UploadController :: UploadTempFile
- ConfigController :: GetLogging
- Deploy tabtapi-test somewhere (and update link in AdminDev)
- Add Middleware to catch exceptions
- Add middleware to log request timings


Testing...
- Emailing (pwd reset)
- Uploading pictures
- SignalR


Performance:
- Only if IsAdmin load the !Active players!
- Cache everything... (need loggedIn & nog logged in caches)
	- Invalidate cache with all changes!
- Review existing table indexes! (gets seem to be like really slow)

## Database

```sh
docker run --name ttc-mysql -p 33060:3306 -e MYSQL_ROOT_PASSWORD=my-secret-pw -d mysql:5.5.60

create database ttc_aalst
-- and load sql script from ./db
```



## EF Migrations

ATTN: We're not using migrations right now!
Might be a good idea to just switch to Postgres and start using this again...



```ps1
cd Ttc.DataAccess

# Install
dotnet tool install --global dotnet-ef

# Create
dotnet ef migrations add InitialCreate
dotnet ef database update

# Delete
dotnet ef migrations remove
dotnet ef database drop -f
```


## New Season

Go to Admin > Params and update the "year" param.

Use `SportaMatchesScoresheetsExcelCreation` "UnitTest" to create the Sporta Excels.

ATTN: Check instead: Perhaps fill in on the site directly?

## Snippets

There is a simple GUI for updating NextKlassementSporta/Vttl but not yet to clear the ones from previous year:
(See commit: 3cbdd71)

```sql
UPDATE speler SET NextKlassementSporta=null, NextKlassementVttl=null;

SELECT Id, NaamKort, KlassementSporta, NextKlassementSporta
FROM speler
WHERE gestopt IS NULL AND klassementsporta IS NOT NULL AND KlassementSporta<>'0'
ORDER BY klassementsporta
```

Match date tempering for in between seasons:

```c#
/// <summary>
/// If there is no real life data between seasons,
/// change some match dates to around now for testing purposes
/// </summary>
private static void RandomizeMatchDatesForTestingPurposes(TtcDbContext context)
{
  bool endOfSeason = !context.Matches.Any(match => match.Date > DateTime.Now);
  if (true || endOfSeason)
  {
    int currentFrenoySeason = context.CurrentFrenoySeason;
    var passedMatches = context.Matches
        .Where(x => x.FrenoySeason == currentFrenoySeason)
        //.Where(x => x.Date < DateTime.Today)
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
```


## Emailing

Go to Admin > Params to update the "SendGridApiKey" param.

Uses a SendGrid API key:  
Username: `pongit`.  
Email: `wouter@pongit.be`  
