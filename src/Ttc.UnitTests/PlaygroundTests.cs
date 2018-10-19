using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using AutoMapper;
using Frenoy.Api;
using NUnit.Framework;
using Ttc.DataAccess;
using Ttc.DataAccess.Services;
using Ttc.DataAccess.Utilities;
using Ttc.DataEntities;
using Ttc.Model;
using Ttc.Model.Clubs;
using Ttc.Model.Matches;
using Ttc.Model.Players;
using Ttc.Model.Teams;
using Ttc.WebApi;

namespace Ttc.UnitTests
{
    [TestFixture]
    public class PlaygroundTests
    {
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
    }
}