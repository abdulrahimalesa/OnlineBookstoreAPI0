namespace OnlineBookstoreAPI0.Models
{
    public class CheckoutRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ShippingAddress { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Status { get; set; } = "Pending";
    }

}
