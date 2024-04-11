using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;


namespace Intextwo.Models
{
    public class Order
    {
        [Key]
        public int transaction_ID { get; set; }

        public int customer_ID { get; set; }
        public DateTime? date { get; set; }
        public string? day_of_week { get; set; }
        public int? time { get; set; }   
        public string? entry_mode { get; set; }
        public int? amount { get; set; }
        public string? type_of_transaction { get; set; }
        public string? shipping_address { get; set; }
        public string? bank { get; set; }
        public string? type_of_card { get; set; }
        public bool? fraud { get; set; }
        public string? country_of_transaction { get; set ; }
        





    }
}
