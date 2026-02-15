using System.ComponentModel.DataAnnotations;

namespace OnlineMarketplace.DTO
{
    public class CreateProductDTO
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int InStock { get; set; }
    }
}
