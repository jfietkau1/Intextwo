using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Intextwo.Models
{
    public class lineItem
    {
        [Key, Column(Order = 0)]
        public int transaction_ID { get; set; }
        public virtual Order Order { get; set; } // Navigation property to Order

        [Key, Column(Order = 1)]
        public int product_ID { get; set; }
        public int? qty { get; set; }
        public int? rating { get; set; }
    }
}
