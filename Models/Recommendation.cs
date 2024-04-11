using System.ComponentModel.DataAnnotations;
namespace Intextwo.Models
{
    public class Recommendation
    {
        [Key]
        public string name { get; set; }
        public string Recommendation1 { get; set; }
        public string Recommendation2 { get; set; }
        public string Recommendation3 { get; set; }
        public string Recommendation4 { get; set; }
        public string Recommendation5 { get; set; }

    }
}
