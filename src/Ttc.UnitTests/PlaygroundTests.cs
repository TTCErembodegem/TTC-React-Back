using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using AutoMapper;
using Frenoy.Api;
using NUnit.Framework;
using Ttc.DataAccess;
using Ttc.DataAccess.App_Start;
using Ttc.DataAccess.Services;
using Ttc.DataAccess.Utilities;
using Ttc.DataEntities;
using Ttc.Model;
using Ttc.Model.Clubs;
using Ttc.Model.Matches;
using Ttc.Model.Players;
using Ttc.Model.Teams;

namespace Ttc.UnitTests
{
    [TestFixture]
    public class PlaygroundTests
    {
        //[Test]
        public void PlayerExcelExport()
        {
            var service = new PlayerService();
            service.GetExcelExport();
        }

        //[Test]
        public void SendToProductionEmails()
        {
            var config = new Dictionary<string, string>
            {
                //["jan.bontinck@gmail.com"] = "secret",
            };

            var here = @"Beste,
<br><br>
De nieuwe website van TCC Erembodegem is een feit!
<br><br>
Er is voor jou speciaal een nieuw paswoord gegenereerd:<br>
<a href=""http://www.ttc-erembodegem.be"">http://www.ttc-erembodegem.be</a><br>
Nieuw paswoord: {0}<br>
<br>
Er is een handleiding hoe alles precies in zijn werk gaat:<br>
<a href=""https://github.com/TTCErembodegem/onboarding/blob/master/README.md"">Online handleiding</a> (met fototjes!)<br>
<br>
Veel success en laat ons weten wat (minder) goed gaat.<br>
<br>
Het TTC-Dev Team,<br>
Wouter & Jorn";

            foreach (var toEmail in config)
            {
                SmtpClient SmtpServer = new SmtpClient("smtp.live.com");
                var mail = new MailMessage();
                mail.From = new MailAddress("woutervs@hotmail.com");
                mail.To.Add(toEmail.Key);
                mail.Subject = "TTC Erembodegem: Paswoord nieuwe website";
                mail.IsBodyHtml = true;
                string htmlBody;
                htmlBody = string.Format(here, toEmail.Value);
                mail.Body = htmlBody;
                SmtpServer.Port = 587;
                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Credentials = new System.Net.NetworkCredential("woutervs@hotmail.com", "");
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
                SmtpServer.Dispose();
            }
        }

        //[Test]
        public void ToProductionEmails()
        {
            using (var context = new TtcDbContext())
            {
                var service = new PlayerService();

                string strSql = "";
                string emails = "";
                var spelers = context.Players.Where(x => x.Gestopt == null).ToArray();
                foreach (var speler in spelers)
                {


                    string newPwd = Path.GetRandomFileName();
                    newPwd = newPwd.Substring(0, newPwd.IndexOf("."));

                    var newCreds = new PasswordCredentials
                    {
                        PlayerId = speler.Id,
                        NewPassword = newPwd
                    };

                    strSql += string.Format($"UPDATE {PlayerEntity.TableName} SET paswoord=MD5(\"{{1}}\") WHERE id={{0}};",
                        newCreds.PlayerId,
                        newCreds.NewPassword)
                        + Environment.NewLine;

                    context.Database.ExecuteSqlCommand(
                        $"UPDATE {PlayerEntity.TableName} SET paswoord=MD5({{1}}) WHERE id={{0}}",
                        newCreds.PlayerId,
                        newCreds.NewPassword);

                    //Console.WriteLine(speler.Email + "," + newPwd);

                    emails += speler.Email.Trim() + "," + newPwd + Environment.NewLine;
                }

                Console.WriteLine(strSql);
                Console.WriteLine(emails);
            }
        }

        //[Test]
        public void TeamMapping()
        {
            using (var dbContext = new TtcDbContext())
            {
                AutoMapperConfig.Configure(new KlassementValueConverter());

                var serv = new TeamService();
                var club = serv.GetForCurrentYear();
                Assert.That(club.First().Year, Is.EqualTo(Constants.CurrentSeason));
            }
        }

        //[Test]
        public void CalendarMapping()
        {
            using (var dbContext = new TtcDbContext())
            {
                AutoMapperConfig.Configure(new KlassementValueConverter());

                var serv = new MatchService();
                var result = serv.GetRelevantMatches();
                var pastMatch = result.First();
                //var pastMatch = serv.GetMatch(1597);
                Assert.That(pastMatch.Players.Count, Is.Not.EqualTo(0), "Players not loaded");
                Assert.That(pastMatch.Games.Count, Is.Not.EqualTo(0), "Games not loaded");
                Assert.That(pastMatch.Score, Is.Not.Null, "Score not set");
            }
        }

        //[Test]
        public void TeamMapping2()
        {
            using (var dbContext = new TtcDbContext())
            {
                AutoMapperConfig.Configure(new KlassementValueConverter());

                var serv = new TeamService();
                var result = serv.GetForCurrentYear();
                var pastMatch = result.First();
                Assert.That(pastMatch.Players.Count, Is.Not.EqualTo(0));
                Assert.That(pastMatch.Opponents.Count, Is.Not.EqualTo(0));
            }
        }

        //[Test]
        public void ClubMapping()
        {
            using (var dbContext = new TtcDbContext())
            {
                AutoMapperConfig.Configure(new KlassementValueConverter());

                var clubs = dbContext.Clubs.Include(x => x.Contacten).Where(x => x.Id == 1 || x.Id == 28).ToList();
                var club = Mapper.Map<IList<ClubEntity>, IList<Club>>(clubs);
                Assert.That(club.First().MainLocation, Is.Not.Null);
                Assert.That(club.First().Managers.Count, Is.Not.EqualTo(0));
            }
        }

        //[Test]
        public void SpelerToPlayerMapping()
        {
            using (var dbContext = new TtcDbContext())
            {
                AutoMapperConfig.Configure(new KlassementValueConverter());
                PlayerEntity speler = dbContext.Players.First();
                Player player = Mapper.Map<PlayerEntity, Player>(speler);
                Assert.That(player.Vttl, Is.Not.Null);
            }
        }
    }
}