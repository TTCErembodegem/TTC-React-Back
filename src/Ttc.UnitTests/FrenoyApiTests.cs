using FrenoyVttl;
using System.Diagnostics;

namespace Ttc.UnitTests
{
    public class FrenoyApiTests
    {
        [Fact]
        public async Task MatchFullDetails_CommentsTimeStamp_CanReadIncorrectTimestamp()
        {
            var frenoy = new FrenoyVttl.TabTAPI_PortTypeClient();
            GetMatchesResponse1 matches = await frenoy.GetMatchesAsync(new GetMatchesRequest1
            {
                GetMatchesRequest = new GetMatchesRequest()
                {
                    DivisionId = "7916",
                    WithDetailsSpecified = true,
                    WithDetails = true,
                    MatchId = "POVLH01/017"
                }
            });

            Debug.Assert(matches.GetMatchesResponse.MatchCount == "1");
        }
    }
}