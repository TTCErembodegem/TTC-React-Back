using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ttc.Model.Core
{
    internal static class Helpers
    {
        /// <summary>
        /// A match is considered started this time before the official start time
        /// </summary>
        private static readonly TimeSpan MatchStarts = new TimeSpan(0, 30, 0);
        public static bool HasMatchStarted(DateTime date) => date - DateTime.Now < MatchStarts;
    }
}
