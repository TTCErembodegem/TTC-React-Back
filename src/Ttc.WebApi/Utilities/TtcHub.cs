using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.SignalR;

namespace Ttc.WebApi.Utilities
{
    public class TtcHub : Hub
    {
        [AllowAnonymous]
        public void BroadcastSnackbar(string message)
        {
            // TODO: broadcast to all but the sender (both methods)
            Clients.All.broadcastSnackbar(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType">match, player, ...</param>
        /// <param name="dataId">The match, player, ... id</param>
        /// <param name="updateType">Updated a comment, a report, the score, ...</param>
        [AllowAnonymous]
        public void BroadcastReload(string dataType, int dataId, string updateType)
        {
            Clients.All.broadcastReload(dataType, dataId, updateType);
        }
    }
}