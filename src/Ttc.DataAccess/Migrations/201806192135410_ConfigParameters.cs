namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConfigParameters : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO parameter (sleutel, value) VALUES ('googleMapsUrl', 'https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d5029.712844711129!2d4.047850828108818!3d50.9263729181754!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x0%3A0xaf1912d655631a00!2sSportcentrum+Schotte!5e0!3m2!1sen!2sbe!4v1529441837848')");
            Sql("INSERT INTO parameter (sleutel, value) VALUES ('location', 'Sportcentrum Schotte, Kapellekensbaan 8, 9320 Aalst')");
            Sql("INSERT INTO parameter(sleutel, value) VALUES('clubBankNr', 'BE55 0016 5927 6744')");
            Sql("INSERT INTO parameter(sleutel, value) VALUES('clubOrgNr', 'BE 0840.545.283')");
            Sql("INSERT INTO parameter(sleutel, value) VALUES('compBalls', 'Xushaofa tt-bal 3-ster SYNTH 40 mm');");
            Sql("INSERT INTO parameter (sleutel, value) VALUES ('frenoyClubIdSporta', '4055')");
            Sql("INSERT INTO parameter (sleutel, value) VALUES ('additionalMembership', 'Een extra €68 komt bovenop het lidgeld dat kan teruggevorderd worden door 4 kaarten voor het eetfestijn te verkopen.')");
            Sql("INSERT INTO parameter (sleutel, value) VALUES ('recreationalMembers', 'Recreanten: €58 voor volwassenen en €18 voor -18 jarigen (+ €102 voor 6 kaarten eetfestijn).')");
            Sql("update parameter set sleutel = 'adultMembership' where sleutel = 'CLUBINFO_VOLWASSENEN'");
            Sql("update parameter set sleutel = 'competitionDays', value = 'Competitie: ma., wo. en vr. 20u' where sleutel = 'CLUBINFO_COMPETITIE'");
            Sql("update parameter set sleutel = 'trainingDays', value = 'Training: di. en do. vanaf 19u30' where sleutel = 'CLUBINFO_TRAINING'");
            Sql("delete from parameter where sleutel in ('updated', 'kalold', 'kalnew', 'training_personen', 'training_kaldesc')");
            Sql("update parameter set sleutel = 'youthMembership' where sleutel = 'CLUBINFO_MIN18JARIGE'");
            Sql("update parameter set sleutel = 'frenoyClubIdVttl' where sleutel = 'Frenoy_ClubId'");
            Sql("update parameter set value='OVL134' where sleutel='frenoyClubIdVttl'");
        }
        
        public override void Down()
        {
        }
    }
}
