namespace Ttc.Model.Players;

public class PlayerStyle
{
    #region Properties
    public int PlayerId { get; set; }
    public string Name { get; set; }
    public string BestStroke { get; set; }
    #endregion

    #region Constructors
    public PlayerStyle()
    {

    }

    public PlayerStyle(int playerId, string styleName, string bestStroke)
    {
        PlayerId = playerId;
        Name = styleName;
        BestStroke = bestStroke;
    }
    #endregion

    public override string ToString() => $"PlayerId={PlayerId}, Name={Name}, BestStroke={BestStroke}";
}
