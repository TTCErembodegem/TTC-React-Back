# TTC-React-Back

WebApi 2 Backend.  
TODO list at: https://github.com/TTCErembodegem/TTC-React-Front/issues

# Frenoy Synchronizer

Synchronizer TTC Erembodegem MySql database with the Frenoy WebServices.

HOW TO
------

- Update the FrenoySyncOptions properties for current year
- Run the Frenoy.Syncer
- Configure Service Reference FrenoyVTTL and change from VTTL to Sporta (or vice versa). 
- (Un)comment Sporta/VTTL part
- Run the Frenoy.Syncer again
- Export the tables: reeks, clubploeg, clubploegspeler and kalender
- Export all new Clubs, if any (Tables: club and clublokaal)

And import on ttc-erembodegem.be...

**Since we will be running .NET on the server, import/export is no longer required :)**

The WSDL Urls 
-------------
**FrenoyVttlWsdlUrl**: http://api.vttl.be/0.7/?wsdl  
**FrenoySportaWsdlUrl**: http://tafeltennis.sporcrea.be/api/?wsdl

Queries
-------
To make exports easier, delete everything generated: 
```
delete from reeks where ID >= 0;
delete from kalender where ID >= 0;
delete from clubploegspeler where ID >= 0;
delete from clubploeg where ID >= 0;
```

ToDo
----
Use dynamic to import VTTL and Sporta in one go.  
Run syncer directly on the server.