﻿namespace OnlineBookstoreAPI0.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } 

        public ICollection<Order> Orders { get; set; }
    public ICollection<CartItem> CartItems { get; set; }
    }
}
