using System;
using System.Collections.Generic;
using System.Linq;
using Ttc.Model.Matches;

namespace Ttc.Model.Core
{
    /// <summary>
    /// Marker attribute for properties that should not be returned to the client
    /// if she is not currently logged in
    /// </summary>
    public class TtcConfidentialAttribute : Attribute
    {
        public ITtcConfidentialStrategy Strategy { get; private set; }

        public TtcConfidentialAttribute(string strategy = null)
        {
            if (strategy == "MATCH")
            {
                Strategy = new TtcConfidentialMatchStrategy();
            } else if (strategy == "MATCH-COMMENTS")
            {
                Strategy = new TtcConfidentialMatchCommentsStrategy();
            }
        }
    }

    /// <summary>
    /// Don't hide players for non logged ins once the match has started
    /// </summary>
    public class TtcConfidentialMatchStrategy : ITtcConfidentialStrategy
    {
        public bool ShouldHide(object data)
        {
            var match = (Match)data;
            if (match.IsPlayed || match.IsSyncedWithFrenoy || Helpers.HasMatchStarted(match.Date))
            {
                return false;
            }
            return true;
        }
    }

    public class TtcConfidentialMatchCommentsStrategy : ITtcConfidentialStrategy
    {
        public bool ShouldHide(object data)
        {
            var comments = ((Match)data).Comments;
            var toRemove = comments.Where(c => c.Hidden).ToArray();
            foreach (var rm in toRemove)
            {
                comments.Remove(rm);
            }
            return false;
        }
    }

    public interface ITtcConfidentialStrategy
    {
        bool ShouldHide(object data);
    }
}