using System.ComponentModel.DataAnnotations;

namespace Intextwo.Models
{
    public class lineItem
    {
        [Key]
        public int transaction_ID { get; set; }
        public int product_ID { get; set; }
        public int? qty { get; set; }
        public int? rating { get; set; }
    }
}
