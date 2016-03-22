using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Ttc.WebApi.Utilities
{
    public class TtcHub : Hub
    {
        public void BroadcastSnackbar(string message)
        {
            Clients.All.broadcastSnackbar(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType">match, player, ...</param>
        /// <param name="data">The match, the player, ...</param>
        /// <param name="updateType">Updated a comment, a report, the score, ...</param>
        public void BroadcastReload(string dataType, object data, string updateType)
        {
            Clients.All.broadcastReload(dataType, data, updateType);
        }
    }
}