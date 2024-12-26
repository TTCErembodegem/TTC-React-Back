using FrenoyVttl;

namespace Ttc.UnitTests;

public class FrenoyApiTests
{
    /// <summary>
    /// Frenoy sends DateTimes as 2024-09-15 09:43:31.
    /// This results in: FormatException: The string '2024-09-15 09:43:31' is not a valid AllXsd value.
    ///
    /// Expected is ISO8601 which has a T between Date and Time.
    ///
    /// The DateTime has been changed into a string in the generated code since
    /// that field was not being mapped anyway.
    /// </summary>
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

        Assert.IsType<string>(matches.GetMatchesResponse.TeamMatchesEntries[0].MatchDetails.CommentEntries[0].Timestamp);
        Assert.Equal("2024-09-15 09:43:31", matches.GetMatchesResponse.TeamMatchesEntries[0].MatchDetails.CommentEntries[0].Timestamp);
    }
}
