using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataAccess.Entities
{
    /// <summary>
    /// Waardes van de klassementen in Vttl/Sporta
    /// </summary>
    internal class Klassement
    {
        /// <summary>
        /// C4, D2, ...
        /// </summary>
        [Key]
        public string Code { get; set; }

        public int WaardeVttl { get; set; }

        public int WaardeSporta { get; set; }

        public override string ToString()
        {
            return $"Code={Code}, WaardeVttl={WaardeVttl}, WaardeSporta={WaardeSporta}";
        }
    }
}
