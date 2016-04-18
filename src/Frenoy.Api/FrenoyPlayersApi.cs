using System.Collections.Generic;
using System.Linq;
using Frenoy.Api.FrenoyVttl;
using Ttc.DataEntities.Core;
using Ttc.Model.Players;
using Ttc.Model.Teams;

namespace Frenoy.Api
{
    public class FrenoyPlayersApi : FrenoyApiBase
    {
        public FrenoyPlayersApi(ITtcDbContext ttcDbContext, Competition comp) : base(ttcDbContext, comp)
        {

        }

        public void SyncPlayers()
        {
            var frenoyPlayers = _frenoy.GetMembers(new GetMembersRequest
            {
                Club = _settings.FrenoyClub
            });

            foreach (MemberEntryType frenoyPlayer in frenoyPlayers.MemberEntries)
            {
                var existingPlayer = _db.Players.SingleOrDefault(ply => ply.ComputerNummerVttl.HasValue && ply.ComputerNummerVttl.Value.ToString() == frenoyPlayer.UniqueIndex);
                if (existingPlayer != null)
                {
                    //existingPlayer.IndexVttl
                }


            }
        }
    }
}