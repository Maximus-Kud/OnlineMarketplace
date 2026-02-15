using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineMarketplace.DTO;
using OnlineMarketplace.Models;
using OnlineMarketplace.Roles;
using System.Globalization;
using System.Security.Claims;

namespace OnlineMarketplace.Controllers
{
    [ApiController]
    [Route("Marketplace")]
    public class MarketplaceController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly LogService _logService;


        public MarketplaceController(AppDbContext context, UserManager<ApplicationUser> userManager, LogService logService)
        {
            _context = context;
            _userManager = userManager;
            _logService = logService;
        }


        [HttpGet]
        public async Task<ActionResult> GetAvailableProducts()
        {
            var products = await _context.Products
                .Where(p => p.IsAvailable && p.InStock > 0)
                .ToListAsync();

            _logService.Add(new LogFile
            {
                Date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                LogLevel = LogFile.LogLevels.INFO,
                User = await _userManager.GetUserAsync(User) ?? new ApplicationUser { },
                Role = User.FindFirstValue(ClaimTypes.Role) ?? RoleNames.Customer,
                Description = "Created GET-Request to see available products"
            });


            return Ok(new
            {
                Message = "Available products",
                Products = products
            });
        }


        [Authorize]
        [HttpPost("order")]
        public async Task<ActionResult> OrderProduct([FromBody] OrderProductDTO orderData)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return NotFound("User ID was not found");


            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User was not found");


            var products = await _context.Products
                .Where(p => orderData.ProductsId.Contains(p.Id))
                .ToListAsync();


            if (products.Count != orderData.ProductsId.Count) return NotFound("Some products were not found");


            decimal totalPrice = 0;


            foreach (var product in products)
            {
                if (product.InStock <= 0) return BadRequest($"Product {product.Name} is out of stock");

                totalPrice += product.Price;
            }


            var order = new OrderedProduct
            {
                Products = System.Text.Json.JsonSerializer.Serialize(orderData.ProductsId),
                UserId = user.Id,
                TotalPrice = totalPrice,
                Status = Statuses.InShoppingCart,
                OrderedAt = DateTime.UtcNow
            };


            _context.OrderedProducts.Add(order);
            await _context.SaveChangesAsync();

            _logService.Add(new LogFile
            {
                Date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                LogLevel = LogFile.LogLevels.INFO,
                User = await _userManager.GetUserAsync(User) ?? new ApplicationUser { },
                Role = User.FindFirstValue(ClaimTypes.Role) ?? RoleNames.Customer,
                Description = "Created POST-Request to buy product",
                Details = $"{order}"
            });


            return Ok(new
            {
                Message = "Order created",
                OrderId = order.Id,
                Total = totalPrice,
                Balance = $"{user.Balance} (Before transaction)"
            });
        }


        [Authorize]
        [HttpGet("getAccountInfo")]
        public async Task<ActionResult> GetAccountInfo()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("User was not found");

            _logService.Add(new LogFile
            {
                Date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                LogLevel = LogFile.LogLevels.INFO,
                User = await _userManager.GetUserAsync(User) ?? new ApplicationUser { },
                Role = User.FindFirstValue(ClaimTypes.Role) ?? RoleNames.Customer,
                Description = "Created GET-Request to get account info",
                Details = $"{user}"
            });

            return Ok(new
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Balance = user.Balance,
                Role = User.FindFirstValue(ClaimTypes.Role)
            });
        }


        [Authorize(Roles = RoleNames.Owner)]
        [HttpGet("getLogs")]
        public async Task<ActionResult> GetLogs()
        {
            return Ok(_logService.Logs);
        }
    }
}
