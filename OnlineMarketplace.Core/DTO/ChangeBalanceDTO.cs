namespace OnlineMarketplace.DTO
{
    public class ChangeBalanceDTO
    {
        public string AccountId { get; set; } = string.Empty;
        public decimal NewBalance { get; set; }
    }
}
