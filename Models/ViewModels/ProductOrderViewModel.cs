namespace Intextwo.Models.ViewModels
{
    public class ProductOrderViewModel
    {
        //below this come from Order class
        public int transaction_ID { get; set; }
        //public int customer_ID { get; set; } // commented this line because it was causing problems over the duplicate field.
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

        //below this come from customer class
        public int customer_ID { get; set; }
        public string? first_name { get; set; }
        public string? last_name { get; set; }
        public DateTime? birth_date { get; set; }
        public string? country_of_residence { get; set; }
        public string? gender { get; set; }
        public decimal? age { get; set; }

    }
}
