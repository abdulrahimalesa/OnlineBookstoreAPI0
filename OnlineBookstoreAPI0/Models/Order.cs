namespace OnlineBookstoreAPI0.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public double TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ShippingAddress { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }

        // OrderDetail'dan alınan yeni alanlar
        public int BookId { get; set; }       // Kitap ID'si
        public string BookTitle { get; set; }  // Kitap başlığı
        public double BookPrice { get; set; }  // Kitap fiyatı
        public int Quantity { get; set; }      // Sipariş miktarı
        public string Status { get; set; }
    }

}
