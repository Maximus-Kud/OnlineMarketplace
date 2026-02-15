namespace OnlineMarketplace.DTO
{
    public class UpdateProductDTO
    {
        public string? Name { get; set; }

        public decimal? Price {  get; set; }

        public int? InStock { get; set; }

        public bool? IsAvailable { get; set; }
    }
}
