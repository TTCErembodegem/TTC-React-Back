namespace Ttc.Model.Clubs
{
    /// <summary>
    /// Voorzitter, secretaris, ...
    /// </summary>
    public class ClubManager
    {
        public int SpelerId { get; set; }
        public string Description { get; set; }

        public override string ToString() => $"SpelerId={SpelerId}, Desc={Description}";
    }
}