using OnlineBookstoreAPI0.Models;

public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int BookCount { get; set; } = 0; 

 
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
