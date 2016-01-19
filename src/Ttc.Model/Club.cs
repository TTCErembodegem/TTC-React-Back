namespace Ttc.Model
{
    public class Club
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CodeVttl { get; set; }
        public string CodeSporta { get; set; }
        public bool Active { get; set; }
        public bool Shower { get; set; }
        public string Website { get; set; }

        public override string ToString()
        {
            return $"Id={Id}, Name={Name}, Vttl={CodeVttl}, Sporta={CodeSporta}, Active={Active}";
        }
    }
}