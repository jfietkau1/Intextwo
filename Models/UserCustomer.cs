using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace Intextwo.Models
{
    //this class is a bridge between the user and customer table because they don't have any matching fields
    public class UserCustomer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int customer_ID { get; set; }
        [Required]
        public string Id { get; set; }

    }
}
