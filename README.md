# TTC-React-Back

WebApi 2 Backend.  


## New Season

Update `Constants.CurrentSeason`.
`DataAccess.NewSeasonSeed` is called from `DataAccess\Migrations\Configuration`.  
ATTN: With the async stuff we'd have to run the seed async. Possible? Simple .Wait() didn't work.
TODO: Move the current season to the db so that a new publish is no longer required...


Use `SportaMatchExcelCreator` to create the Sporta matches.
