namespace OnlineMarketplace.Models
{
    public class OrderedProduct
    {
        public int Id { get; set; }

        public string Products { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public decimal TotalPrice { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime OrderedAt { get; set; }
    }
}
