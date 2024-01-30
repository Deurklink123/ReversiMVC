using ReversiMvcApp.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReversiMvcApp.Models
{
    public class Spel
    {
        [Key]
        public string token { get; set; }
        public string omschrijving { get; set; }
        [NotMapped]
        public Kleur[,] bord { get; set; }
        public string bordString { get; set; }
        public string aanDeBeurt { get; set; }
    }
}
