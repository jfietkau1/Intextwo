using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Intextwo.Models
{
    public class Customer
    {
        [Key]
        public int customer_ID {  get; set; }
        public string? first_name { get; set; }
        public string? last_name { get; set; }
        public DateTime? birth_date { get; set; }
        public string? country_of_residence { get; set; }
        public string? gender { get; set; }
        public decimal? age { get; set; }
        public int? recentprod { get; set; }

    }
}
