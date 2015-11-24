using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FrenoySyncer.FrenoyVttl;
using Ttc.DataAccess;
using Ttc.Model;

namespace FrenoySyncer
{
    public class FrenoySync : IDisposable
    {
        #region Fields
        const string FrenoyVttlWsdlUrl = "http://api.vttl.be/0.7/?wsdl";
        const string FrenoySportaWsdlUrl = "http://tafeltennis.sporcrea.be/api/?wsdl";

        private readonly TtcDbContext _db;
        private readonly FrenoySyncOptions _options;
        private readonly TabTAPI_PortTypeClient _frenoy;
        private readonly int _thuisClubId;
        private readonly bool _isVttl;
        //private readonly FileInfo _logFileInfo;
        //private readonly StreamWriter _logFile;
        #endregion

        #region Constructor
        public FrenoySync(FrenoySyncOptions options, bool isVttl = true)
        {
            _db = new TtcDbContext();
            // TODO: Pointless to use the EF logging: Parameter values are not part of the output...
            //_logFileInfo = new FileInfo(@"C:\temp\log" + DateTime.Now.ToString("hh:mm:ss").Replace(":", "") + ".txt");
            //_logFile = new StreamWriter(_logFileInfo.FullName);
            //_db.Database.Log = message => _log.AppendLine(message);

            // TODO: The logs contain parameters without the values, so the queries are useless
            // -> Perhaps Glimpse can help here? It got some parameter replacement thingie

            _options = options;
            CheckPlayers();

            _isVttl = isVttl;
            //string wsdl;
            if (isVttl)
            {
                _thuisClubId = _db.Clubs.Single(x => x.CodeVTTL == options.FrenoyClub).ID;
                //wsdl = FrenoyVttlWsdlUrl;
            }
            else
            {
                // Sporta
                _thuisClubId = _db.Clubs.Single(x => x.CodeSporta == options.FrenoyClub).ID;
                //wsdl = FrenoySportaWsdlUrl;
            }

            // Aparently the signatures for VTTL and Sporta are not identical
            // Problem is probably stuff like: xmlns="http://api.frenoy.net/TabTAPI" in the body
            //var binding = new BasicHttpBinding();
            //binding.Security.Mode = BasicHttpSecurityMode.None;
            //var endpoint = new EndpointAddress(wsdl);
            //_frenoy = new TabTAPI_PortTypeClient(binding, endpoint);

            // Right click the Service Reference and update with different Url...
            _frenoy = new FrenoyVttl.TabTAPI_PortTypeClient();
        }

        [Conditional("DEBUG")]
        private void CheckPlayers()
        {
            foreach (string player in _options.Players.Values.SelectMany(x => x))
            {
                try
                {
                    GetSpelerId(player);
                }
                catch (Exception)
                {
                    throw new Exception("No player with NaamKort " + player);
                }
            }
        }
        #endregion

        #region Public API
        public void WriteLog()
        {
            //var queries = _log.ToString();

            //var nonQuery = new Regex(@"^(Opened connection|Started transaction|Committed transaction|Closed connection|Disposed transaction|--).*$", RegexOptions.Multiline);
            //queries = nonQuery.Replace(queries, "");
            //queries = queries.Replace("SET SESSION sql_mode='ANSI';", "");
            //queries = Regex.Replace(queries, @"(\r|\n){2,}", "\r\n");

            //File.WriteAllText(@"C:\temp\log" + DateTime.Now.ToString("hh:mm:ss").Replace(":", "") + "_" + (_isVttl ? "VTTL" : "Sporta") + ".txt", queries);
        }

        public void Sync()
        {
            var frenoyTeams = _frenoy.GetClubTeams(new GetClubTeamsRequest
            {
                Club = _options.FrenoyClub,
                Season = _options.FrenoySeason
            });

            foreach (var frenoyTeam in frenoyTeams.TeamEntries)
            {
                // Create new division=reeks for each team in the club
                // Check if it already exists: Two teams could play in the same division
                Reeks reeks = _db.Reeksen.SingleOrDefault(x => x.FrenoyDivisionId.ToString() == frenoyTeam.DivisionId);
                if (reeks == null)
                {
                    reeks = CreateReeks(frenoyTeam);
                    _db.Reeksen.Add(reeks);
                    CommitChanges();

                    // Create the teams in the new division=reeks
                    var frenoyDivision = _frenoy.GetDivisionRanking(new GetDivisionRankingRequest
                    {
                        DivisionId = frenoyTeam.DivisionId
                    });
                    foreach (var frenoyTeamsInDivision in frenoyDivision.RankingEntries)
                    {
                        var clubPloeg = CreateClubPloeg(reeks, frenoyTeamsInDivision);
                        _db.ClubPloegen.Add(clubPloeg);
                    }
                    CommitChanges();
                }
                
                

                // Add Erembodegem players to the home team
                var ploeg = _db.ClubPloegen.Single(x => x.ClubId == _thuisClubId && x.ReeksId == reeks.ID && x.Code == frenoyTeam.Team);
                var players = _options.Players[ploeg.Code];
                foreach (var playerName in players)
                {
                    var clubPloegSpeler = new ClubPloegSpeler
                    {
                        Kapitein = playerName == players.First() ? 1 : 0,
                        SpelerID = GetSpelerId(playerName),
                        ClubPloegID = ploeg.ID
                    };
                    _db.ClubPloegSpelers.Add(clubPloegSpeler);
                }
                CommitChanges();



                // Create the matches=kalender table in the new  division=reeks
                var matches = _frenoy.GetMatches(new GetMatchesRequest
                {
                    Club = _options.FrenoyClub,
                    Season = _options.FrenoySeason,
                    DivisionId = reeks.FrenoyDivisionId.ToString(),
                    Team = ploeg.Code
                });
                foreach (var frenoyMatch in matches.TeamMatchesEntries.Where(x => x.HomeTeam.Trim() != "Vrij" && x.AwayTeam.Trim() != "Vrij"))
                {
                    Debug.Assert(frenoyMatch.DateSpecified);
                    Debug.Assert(frenoyMatch.TimeSpecified);

                    Kalender kalender = CreateKalenderMatch(reeks, frenoyMatch, ploeg.Code);
                    _db.Kalender.Add(kalender);
                }
                CommitChanges();
            }
        }
        #endregion

        #region Logging
        //private readonly StringBuilder _log = new StringBuilder();
        private void CommitChanges()
        {
            //_db.Database.Log = Console.Write;
            //_db.Database.Log = message => _log.AppendLine(message);
            //_db.Database.Log = _logFile.Write;
            _db.SaveChanges();
            //_db.Database.Log = null;
        }
        #endregion

        #region Private Implementation
        private int GetSpelerId(string playerName)
        {
            var speler = _db.Spelers.Single(x => x.NaamKort == playerName);
            return speler.ID;
        }

        private readonly static Regex VttlReeksRegex = new Regex(@"Afdeling (\d+)(\w+)");
        private readonly static Regex SportaReeksRegex = new Regex(@"^(\d)(\w?)");
        private Reeks CreateReeks(TeamEntryType frenoyTeam)
        {
            var reeks = new Reeks();
            reeks.Competitie = _options.Competitie;
            reeks.ReeksType = _options.ReeksType;
            reeks.Jaar = _options.Jaar;
            reeks.LinkID = $"{frenoyTeam.DivisionId}_{frenoyTeam.Team}";

            if (_isVttl)
            {
                var reeksMatch = VttlReeksRegex.Match(frenoyTeam.DivisionName);
                reeks.ReeksNummer = reeksMatch.Groups[1].Value;
                reeks.ReeksCode = reeksMatch.Groups[2].Value;
            }
            else
            {
                var reeksMatch = SportaReeksRegex.Match(frenoyTeam.DivisionName.Trim());
                reeks.ReeksNummer = reeksMatch.Groups[1].Value;
                reeks.ReeksCode = reeksMatch.Groups[2].Value;
            }

            reeks.FrenoyDivisionId = int.Parse(frenoyTeam.DivisionId);
            reeks.FrenoyTeamId = frenoyTeam.TeamId;
            return reeks;
        }

        private Kalender CreateKalenderMatch(Reeks reeks, TeamMatchEntryType frenoyMatch, string thuisPloegCode)
        {
            var kalender = new Kalender
            {
                FrenoyMatchId = frenoyMatch.MatchId,
                Datum = frenoyMatch.Date,
                Uur = frenoyMatch.Time,
                ThuisClubID = GetClubId(frenoyMatch.HomeClub),
                ThuisPloeg = ExtractTeamCodeFromFrenoyName(frenoyMatch.HomeTeam),
                UitClubID = GetClubId(frenoyMatch.AwayClub),
                UitPloeg = ExtractTeamCodeFromFrenoyName(frenoyMatch.AwayTeam),
                Week = int.Parse(frenoyMatch.WeekName)
            };

            kalender.ThuisClubPloegID = GetClubPloegId(reeks.ID, kalender.ThuisClubID.Value, kalender.ThuisPloeg);
            kalender.UitClubPloegID = GetClubPloegId(reeks.ID, kalender.UitClubID.Value, kalender.UitPloeg);

            // In the db the ThuisClubId is always Erembodegem
            kalender.Thuis = kalender.ThuisClubID == _thuisClubId && kalender.ThuisPloeg == thuisPloegCode ? 1 : 0;
            if (kalender.Thuis == 0)
            {
                var thuisClubId = kalender.ThuisClubID;
                var thuisPloeg = kalender.ThuisPloeg;
                var thuisClubPloegId = kalender.ThuisClubPloegID;

                kalender.ThuisClubID = kalender.UitClubID;
                kalender.ThuisPloeg = kalender.UitPloeg;
                kalender.ThuisClubPloegID = kalender.UitClubPloegID;

                kalender.UitClubID = thuisClubId;
                kalender.UitPloeg = thuisPloeg;
                kalender.UitClubPloegID = thuisClubPloegId;
            }
            return kalender;
        }

        private int GetClubPloegId(int reeksId, int clubId, string ploeg)
        {
            var cb = _db.ClubPloegen.Single(x => x.ReeksId == reeksId && x.ClubId == clubId && x.Code == ploeg);
            return cb.ID;
        }

        private ClubPloeg CreateClubPloeg(Reeks reeks, RankingEntryType frenoyTeam)
        {
            var clubPloeg = new ClubPloeg();
            clubPloeg.ReeksId = reeks.ID;
            clubPloeg.ClubId = GetClubId(frenoyTeam.TeamClub);
            clubPloeg.Code = ExtractTeamCodeFromFrenoyName(frenoyTeam.Team);
            return clubPloeg;
        }

        private static readonly Regex ClubHasTeamCodeRegex = new Regex(@"\w$");
        private static string ExtractTeamCodeFromFrenoyName(string team)
        {
            if (ClubHasTeamCodeRegex.IsMatch(team))
            {
                return team.Substring(team.Length - 1);
            }
            Debug.Assert(false, "This code path is never been tested");
            return null;
        }

        private int GetClubId(string frenoyClubCode)
        {
            Club club;
            if (_isVttl)
            {
                club = _db.Clubs.SingleOrDefault(x => x.CodeVTTL == frenoyClubCode);
            }
            else
            {
                club = _db.Clubs.SingleOrDefault(x => x.CodeSporta == frenoyClubCode);
            }
            if (club == null)
            {
                club = CreateClub(frenoyClubCode);
            }
            return club.ID;
        }

        private Club CreateClub(string frenoyClubCode)
        {
            Debug.Assert(_isVttl, "or need to write an if");
            var frenoyClub = _frenoy.GetClubs(new GetClubs
            {
                Club = frenoyClubCode,
                Season = _options.FrenoySeason
            });
            Debug.Assert(frenoyClub.ClubEntries.Count() == 1);

            var club = new Club
            {
                CodeVTTL = frenoyClubCode,
                Actief = 1,
                Naam = frenoyClub.ClubEntries.First().LongName,
                Douche = 0
            };
            _db.Clubs.Add(club);
            CommitChanges();

            foreach (var frenoyLokaal in frenoyClub.ClubEntries.First().VenueEntries)
            {
                var lokaal = new ClubLokaal
                {
                    ClubId = club.ID,
                    Telefoon = frenoyLokaal.Phone,
                    Lokaal = frenoyLokaal.Name,
                    Adres = frenoyLokaal.Street,
                    Postcode = int.Parse(frenoyLokaal.Town.Substring(0, frenoyLokaal.Town.IndexOf(" "))),
                    Gemeente = frenoyLokaal.Town.Substring(frenoyLokaal.Town.IndexOf(" ") + 1),
                    Hoofd = 1
                };
                _db.ClubLokalen.Add(lokaal);
            }

            return club;
        }
        #endregion

        public void Dispose()
        {
            _db.Dispose();
            //_logFile.Close();
        }
    }
}