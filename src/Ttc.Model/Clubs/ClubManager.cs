using Ttc.Model.Players;

namespace Ttc.Model.Clubs
{
    public enum ClubManagerType
    {
        Default,
        Chairman,
        Secretary,
        Treasurer
    }

    /// <summary>
    /// Voorzitter, secretaris, ...
    /// </summary>
    public class ClubManager
    {
        public int PlayerId { get; set; }
        public ClubManagerType Description { get; set; }
        public string Name { get; set; }
        public PlayerContact Contact { get; set; }
        public int SortOrder { get; set; }

        public override string ToString() => $"Player={Name} ({PlayerId}), Desc={Description}";
    }
}