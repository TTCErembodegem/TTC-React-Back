namespace Ttc.Model.Players
{
    public class PlayerStyle
    {
        #region Properties
        public string Name { get; }
        public string BestStroke { get; }
        #endregion

        #region Constructor
        public PlayerStyle()
        {
            
        }

        public PlayerStyle(string styleName, string bestStroke)
        {
            Name = styleName;
            BestStroke = bestStroke;
        }
        #endregion

        public override string ToString() => $"Name={Name}, BestStroke={BestStroke}";
    }
}