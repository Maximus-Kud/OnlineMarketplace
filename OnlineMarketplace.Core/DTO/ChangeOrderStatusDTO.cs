namespace OnlineMarketplace.DTO
{
    public class ChangeOrderStatusDTO
    {
        public int OrderId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
