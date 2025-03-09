using OnlineBookstoreAPI0.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public int GenreId { get; set; }

 
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
 }
