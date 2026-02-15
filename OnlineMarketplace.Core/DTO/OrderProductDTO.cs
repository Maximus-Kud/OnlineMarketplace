namespace OnlineMarketplace.DTO
{
    public class OrderProductDTO
    {
        public List<int> ProductsId { get; set; } = new List<int>();
        public int ProductAmount { get; set; }
    }
}
