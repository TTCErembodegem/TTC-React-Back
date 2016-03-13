using System;
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
            if (match.IsPlayed || match.ScoreType == MatchOutcome.BeingPlayed)
            {
                return false;
            }
            return true;
        }
    }

    public interface ITtcConfidentialStrategy
    {
        bool ShouldHide(object data);
    }
}