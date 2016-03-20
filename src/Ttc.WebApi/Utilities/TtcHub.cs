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

        public void BroadcastReload(string type, object data)
        {
            Clients.All.broadcastReload(type, data);
        }
    }
}