using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineMarketplace.DTO;
using OnlineMarketplace.DTO.ResponseDTO;
using OnlineMarketplace.Models;
using OnlineMarketplace.Roles;
using System.Globalization;

namespace OnlineMarketplace.Controllers
{
    [ApiController]
    [Route("Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly LogService _logService;

        public AdminController(AppDbContext context, UserManager<ApplicationUser> userManager, LogService logService)
        {
            _context = context;
            _userManager = userManager;
            _logService = logService;
        }


        [Authorize(Roles = RoleNames.Admin)]
        [HttpGet("products")]
        public async Task<ActionResult> GetAllProducts()
        {
            var products = await _context.Products.ToListAsync();

            _logService.Add(new LogFile
            {
                Date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                LogLevel = LogFile.LogLevels.WARNING,
                User = await _userManager.GetUserAsync(User) ?? new ApplicationUser { },
                Role = RoleNames.Admin,
                Description = "Admin created GET-Request to see all available products"
            });


            return Ok(products);
        }


        [Authorize(Roles = RoleNames.Admin)]
        [HttpPost]
        public async Task<ActionResult> AddNewProduct([FromBody] CreateProductDTO createProductData)
        {
            if (!ModelState.IsValid) return BadRequest("Invalid input");

            var product = new Product
            {
                Name = createProductData.Name,
                Price = createProductData.Price,
                InStock = createProductData.InStock,
                IsAvailable = true
            };


            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            _logService.Add(new LogFile
            {
                Date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                LogLevel = LogFile.LogLevels.WARNING,
                User = await _userManager.GetUserAsync(User) ?? new ApplicationUser { },
                Role = RoleNames.Admin,
                Description = "Admin created POST-Request to create a new product",
                Details = $"{product}"
            });


            return Ok(new
            {
                Message = "Product created",
                Product = product
            });
        }


        [Authorize(Roles = RoleNames.Admin)]
        [HttpPatch("updateProduct/{id}")]
        public async Task<ActionResult> UpdateProduct(int id, [FromBody] UpdateProductDTO updateProductData)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound("Product was not found");

            bool noNameChange = updateProductData.Name == null || updateProductData.Name == product.Name;

            bool noPriceChange = !updateProductData.Price.HasValue || updateProductData.Price.Value == product.Price;

            bool noStockChange = !updateProductData.InStock.HasValue || updateProductData.InStock.Value == product.InStock;

            bool noAvailabilityChange = !updateProductData.IsAvailable.HasValue || updateProductData.IsAvailable.Value == product.IsAvailable;

            if (noNameChange && noPriceChange && noStockChange && noAvailabilityChange)
                return BadRequest("No changes were made");


            if (!noNameChange) product.Name = updateProductData.Name!;
            if (!noPriceChange) product.Price = updateProductData.Price!.Value;
            if (!noStockChange) product.InStock = updateProductData.InStock!.Value;
            if (!noAvailabilityChange) product.IsAvailable = updateProductData.IsAvailable!.Value;

            await _context.SaveChangesAsync();

            _logService.Add(new LogFile
            {
                Date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                LogLevel = LogFile.LogLevels.WARNING,
                User = await _userManager.GetUserAsync(User) ?? new ApplicationUser { },
                Role = RoleNames.Admin,
                Description = "Admin created PATCH-Request to update the product",
                Details = $"Product ID: {id} was successfully updated>\n{product}"
            });


            return Ok(new
            {
                Message = $"Product ID: {id} was successfully updated",
                Product = product
            });
        }


        [Authorize(Roles = RoleNames.Admin)]
        [HttpDelete("deleteProduct/{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound("Product was not found");

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            _logService.Add(new LogFile
            {
                Date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                LogLevel = LogFile.LogLevels.WARNING,
                User = await _userManager.GetUserAsync(User) ?? new ApplicationUser { },
                Role = RoleNames.Admin,
                Description = "Admin created DELETE-Request to delete the product",
                Details = $"Product was successfully removed from the table>\n{product}"
            });


            return Ok(new DeletedProductResponseDTO
            {
                Message = "Product was successfully removed from the table",
                DeletedProduct = product
            });
        }


        [Authorize(Roles = RoleNames.Admin)]
        [HttpGet("users")]
        public async Task<ActionResult> GetAllUsers()
        {
            var customers = await _userManager.GetUsersInRoleAsync(RoleNames.Customer);
            var admins = await _userManager.GetUsersInRoleAsync(RoleNames.Admin);

            _logService.Add(new LogFile
            {
                Date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                LogLevel = LogFile.LogLevels.WARNING,
                User = await _userManager.GetUserAsync(User) ?? new ApplicationUser { },
                Role = RoleNames.Admin,
                Description = "Admin created GET-Request to get all users"
            });


            return Ok(new
            {
                Customers = customers,
                Admins = admins
            });
        }


        [Authorize(Roles = RoleNames.Admin)]
        [HttpGet("getOrdersInShoppingCart")]
        public async Task<ActionResult> GetOrdersInShoppingCart()
        {
            var orders = await _context.OrderedProducts
                .Where(p => p.Status == Statuses.InShoppingCart)
                .ToListAsync();

            _logService.Add(new LogFile
            {
                Date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                LogLevel = LogFile.LogLevels.WARNING,
                User = await _userManager.GetUserAsync(User) ?? new ApplicationUser { },
                Role = RoleNames.Admin,
                Description = "Admin created GET-Request to get orders in shopping cart"
            });


            return Ok(orders);
        }


        [Authorize(Roles = RoleNames.Admin)]
        [HttpGet("getOrdersPurchased")]
        public async Task<ActionResult> GetOrdersPurchased()
        {
            var orders = await _context.OrderedProducts
                .Where(p => p.Status == Statuses.Purchased)
                .ToListAsync();

            _logService.Add(new LogFile
            {
                Date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                LogLevel = LogFile.LogLevels.WARNING,
                User = await _userManager.GetUserAsync(User) ?? new ApplicationUser { },
                Role = RoleNames.Admin,
                Description = "Admin created GET-Request to get purchased orders"
            });


            return Ok(orders);
        }


        [Authorize(Roles = RoleNames.Admin)]
        [HttpPatch("changeOrderStatus")]
        public async Task<ActionResult> ChangeOrderStatus([FromBody] ChangeOrderStatusDTO changeOrderStatusData)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.OrderedProducts.FindAsync(changeOrderStatusData.OrderId);

                if (order == null) return NotFound($"Order ID: {changeOrderStatusData.OrderId} was not found");
                if (order.Status == Statuses.Purchased) return BadRequest("Order was already purchased");
                if (order.Status == Statuses.Cancelled) return BadRequest("Cancelled order cannot be changed");
                if (order.Status == changeOrderStatusData.Status) return BadRequest($"Order ID: {order.Id} already had this status: {order.Status}");


                if (changeOrderStatusData.Status == Statuses.Purchased)
                {
                    var user = await _context.Users.FindAsync(order.UserId);
                    if (user == null) return NotFound($"User of the order {order.Id} was not found");

                    if (user.Balance < order.TotalPrice) return BadRequest($"User does not have enough money for this order ID: {order.Id}");


                    List<int>? productsIds;
                    try
                    {
                        productsIds = string.IsNullOrEmpty(order.Products) ? null : System.Text.Json.JsonSerializer.Deserialize<List<int>>(order.Products);
                    }
                    catch (System.Text.Json.JsonException)
                    {
                        return BadRequest("Invalid input. Products' ids must be a list");
                    }

                    if (productsIds == null || !productsIds.Any()) return BadRequest("No products");


                    var products = await _context.Products
                        .Where(p => productsIds.Contains(p.Id))
                        .ToListAsync();


                    foreach (var product in products)
                    {
                        if (product.InStock <= 0) return BadRequest($"Product {product.Name} is out of stock");
                        if (product.IsAvailable == false) return BadRequest($"Product is currently not available");
                    }


                    foreach (var product in products)
                    {
                        product.InStock--;

                        if (product.InStock == 0) product.IsAvailable = false;
                    }


                    user.Balance -= order.TotalPrice;
                }


                order.Status = changeOrderStatusData.Status;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logService.Add(new LogFile
                {
                    Date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                    LogLevel = LogFile.LogLevels.WARNING,
                    User = await _userManager.GetUserAsync(User) ?? new ApplicationUser { },
                    Role = RoleNames.Admin,
                    Description = "Admin created PATCH-Request to change order's status",
                    Details = $"Successfully changed order status to {order.Status}\n{order}"
                });


                return Ok(new
                {
                    Message = $"Successfully changed order status to {order.Status}",
                    Order = order
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, "Server error " + ex.Message);
            }
        }


        [Authorize(Roles = RoleNames.Admin)]
        [HttpPatch("changeAccountBalance")]
        public async Task<ActionResult> ChangeAccountBalance([FromBody] ChangeBalanceDTO changeBalanceData)
        {
            var account = await _userManager.FindByIdAsync(changeBalanceData.AccountId);
            if (account == null) return NotFound("User not found");

            if (changeBalanceData.NewBalance < 0) return BadRequest("Balance cannot be negative");

            account.Balance = changeBalanceData.NewBalance;

            await _context.SaveChangesAsync();

            _logService.Add(new LogFile
            {
                Date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                LogLevel = LogFile.LogLevels.WARNING,
                User = await _userManager.GetUserAsync(User) ?? new ApplicationUser { },
                Role = RoleNames.Admin,
                Description = "Admin created PATCH-Request to change account's status",
                Details = $"Balance updated {account.Id}\n{account.Balance}"
            });

            return Ok(new ChangeAccountBalanceResponseDTO
            {
                Message = "Balance updated",
                AccountId = account.Id,
                Balance = account.Balance
            });
        }
    }
}
