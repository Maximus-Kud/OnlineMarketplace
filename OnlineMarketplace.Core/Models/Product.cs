using System.ComponentModel.DataAnnotations;

namespace OnlineMarketplace.Models
{
    public class Product
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MinLength(5)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int InStock { get; set; }

        public bool IsAvailable { get; set; }
    }
}
