namespace Ttc.Model.Players
{
    public class PlayerStyle
    {
        public string Name { get; }
        public string BestStroke { get; }

        public PlayerStyle()
        {
            
        }

        public PlayerStyle(string styleName, string bestStroke)
        {
            Name = styleName;
            BestStroke = bestStroke;
        }

        public override string ToString()
        {
            return $"Name={Name}, BestStroke={BestStroke}";
        }
    }
}