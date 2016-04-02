using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ttc.Model.Players
{
    public class PlayerContact
    {
        #region Properties
        public int PlayerId { get; set; }
        public string Email { get; set; }
        public string GSM { get; set; }
        #endregion

        #region Constructor
        public PlayerContact()
        {

        }

        public PlayerContact(int playerId, string eMail, string gsm)
        {
            PlayerId = playerId;
            Email = eMail;
            GSM = gsm;
        }
        #endregion

        public override string ToString() => $"PlayerId={PlayerId}, EMail={Email}, Mobile={GSM}";
    }
}
