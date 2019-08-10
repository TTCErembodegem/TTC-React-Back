# TTC-React-Back

WebApi 2 Backend.  


## New Season

Go to Admin > Params and update the "year" param.

Use `SportaMatchesScoresheetsExcelCreation` "UnitTest" to create the Sporta Excels.


## Snippets

There is no GUI for updating NextKlassementSporta/Vttl yet:

```sql
SELECT Id, NaamKort, KlassementSporta, NextKlassementSporta
FROM speler
WHERE gestopt IS NULL AND klassementsporta IS NOT NULL AND KlassementSporta<>'0'
ORDER BY klassementsporta 
```