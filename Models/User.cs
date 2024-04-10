using System.ComponentModel.DataAnnotations;

namespace brickit.Models
{
    public class User
    {
        [Key]
        public int user_ID { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public int? customer_ID { get; set; }
        public string auth_status { get; set; }
    }
}
