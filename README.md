# TTC-React-Back

WebApi 2 Backend.  

## Deploy

[Jenkins](http://pongit:9001/job/ttc-front/)


## New Season

Go to Admin > Params and update the "year" param.

Use `SportaMatchesScoresheetsExcelCreation` "UnitTest" to create the Sporta Excels.

ATTN: Check instead: Perhaps fill in on the site directly?

## Snippets

There is a simple GUI for updating NextKlassementSporta/Vttl but not yet to clear the ones from previous year:

```sql
UPDATE speler SET NextKlassementSporta=null, NextKlassementVttl=null;

SELECT Id, NaamKort, KlassementSporta, NextKlassementSporta
FROM speler
WHERE gestopt IS NULL AND klassementsporta IS NOT NULL AND KlassementSporta<>'0'
ORDER BY klassementsporta
```

## Emailing

Go to Admin > Params to update the "SendGridApiKey" param.

Uses a SendGrid API key:  
Username: `pongit`.  
Email: `wouter@pongit.be`  
