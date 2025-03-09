namespace OnlineBookstoreAPI0.Data
{
    using Microsoft.EntityFrameworkCore;
    using OnlineBookstoreAPI0.Models;

    public class OnlineBookstoreContext : DbContext
    {
        public OnlineBookstoreContext(DbContextOptions<OnlineBookstoreContext> options)
            : base(options) { }

        // DbSet properties for each model
        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Book>().ToTable("Books");
            modelBuilder.Entity<Genre>().ToTable("Genres");
            modelBuilder.Entity<CartItem>().ToTable("CartItems");
            modelBuilder.Entity<Order>().ToTable("Orders");
  
            modelBuilder.Entity<Book>()
                .Property(b => b.Price)
                .HasColumnType("decimal(18,2)"); 

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)"); 

            
        }
    }
}
