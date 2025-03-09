using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineBookstoreAPI0.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; } 



         //public User User { get; set; }

         //public Book Book { get; set; }
    }
}
