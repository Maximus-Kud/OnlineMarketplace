namespace OnlineMarketplace.DTO.ResponseDTO
{
    public class ChangeAccountBalanceResponseDTO
    {
        public string Message { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }
}
