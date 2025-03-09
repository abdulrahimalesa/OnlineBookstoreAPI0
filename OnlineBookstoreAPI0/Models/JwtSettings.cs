namespace OnlineBookstoreAPI0.Models
{
    public class JwtSettings
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string ExpiresIn { get; set; }
    }
}
