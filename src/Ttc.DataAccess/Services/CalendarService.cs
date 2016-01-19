using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AutoMapper;
using Ttc.DataAccess.Entities;
using Ttc.Model;

namespace Ttc.DataAccess.Services
{
    public class CalendarService
    {
        public IEnumerable<Match> GetRelevantCalendarItems()
        {
            using (var dbContext = new TtcDbContext())
            {
                var dateBegin = DateTime.Now.AddDays(-7);
                var dateEnd = DateTime.Now.AddDays(7);

                var calendar = dbContext.Kalender
                    .Where(x => x.Datum >= dateBegin)
                    .Where(x => x.Datum <= dateEnd)
                    .Where(x => x.ThuisClubId.HasValue)
                    .ToList();

                //     "SELECT Week, TIME_FORMAT(Uur, '%k:%i') AS Uur, DATE_FORMAT(Datum, '%d/%m/%Y') AS FDatum, kalender.Beschrijving, UitPloeg, DAYOFWEEK(Datum) AS Dag
                //, ThuisClubPloegID, clubthuis.Naam AS ThuisNaam, ThuisPloeg, Competitie, Reeks, ReeksType, ReeksCode, reeks.ID AS ReeksID
                //, UitClubPloegID, clubuit.Naam AS UitNaam, TO_DAYS(Datum)-TO_DAYS(NOW()) AS Vandaag, Thuis, WEEK(Datum) AS JaarWeek, GeleideTraining
                //, v.ID AS VerslagID, v.UitslagThuis, v.UitslagUit, v.WO, v.Details AS HeeftVerslag, kalender.ID AS KalenderID, clubthuis.ID AS ThuisClubID, clubuit.ID AS UitClubID
                //, FrenoyMatchId
                //FROM kalender
                //LEFT JOIN clubploeg thuis ON ThuisClubPloegID=thuis.ID
                //LEFT JOIN reeks ON thuis.ReeksID=reeks.ID
                //LEFT JOIN club clubthuis ON ThuisClubID=clubthuis.ID
                //LEFT JOIN club clubuit ON UitClubID=clubuit.ID
                //LEFT JOIN verslag v ON kalender.ID=v.KalenderID
                //WHERE Datum BETWEEN DATE_SUB(NOW(), INTERVAL ".($params[PARAM_KAL_WEEKS_OLD]*7)." DAY) AND DATE_ADD(NOW(), INTERVAL ".($params[PARAM_KAL_WEEKS_NEW]*7)." DAY)"
                //ORDER BY Datum, kalender.ID");


                var result = Mapper.Map<IList<Kalender>, IList<Match>>(calendar);
                return result;
            }
        }
    }
}