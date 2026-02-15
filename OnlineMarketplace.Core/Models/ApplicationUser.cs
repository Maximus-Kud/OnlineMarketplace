using Microsoft.AspNetCore.Identity;

namespace OnlineMarketplace.Models
{
    public class ApplicationUser : IdentityUser
    {
        public decimal Balance { get; set; }
    }
}
