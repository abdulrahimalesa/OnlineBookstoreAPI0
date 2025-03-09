using Microsoft.AspNetCore.Identity;

namespace OnlineBookstoreAPI0.Models
{
    public class Role : IdentityRole<int>
    {
        public ICollection<User> Users { get; set; } // Kullanıcıları bu role atar
    }
}