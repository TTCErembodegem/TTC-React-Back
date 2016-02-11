namespace Ttc.Model.Clubs
{
    /// <summary>
    /// Voorzitter, secretaris, ...
    /// </summary>
    public class ClubManager
    {
        public int PlayerId { get; set; }
        public string Description { get; set; }

        public override string ToString() => $"PlayerId={PlayerId}, Desc={Description}";
    }
}