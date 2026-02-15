using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using OnlineMarketplace.Controllers;
using OnlineMarketplace.DTO;
using OnlineMarketplace.DTO.ResponseDTO;
using OnlineMarketplace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineMarketplace.Test
{
    public class AdminControllerTest
    {
        private static Mock<UserManager<ApplicationUser>> MockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();

            return new Mock<UserManager<ApplicationUser>>(
                store.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );
        }



        // ---------------------------------------- GetAllProducts ----------------------------------------
        [Fact]
        public async Task GetAllProducts_Return_Array()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);


            var products = await controller.GetAllProducts();


            Assert.IsType<OkObjectResult>(products);
        }



        // ---------------------------------------- AddNewProduct ----------------------------------------
        [Fact]
        public async Task AddNewProduct_Is_Not_Valid_Model()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);

            var dto = new CreateProductDTO
            {
                Name = "Test Product",
                Price = 10,
            };

            controller.ModelState.AddModelError("InStock", "Required");


            var result = await controller.AddNewProduct(dto);


            var resultBadRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal("Invalid input", resultBadRequest.Value);
        }


        [Fact]
        public async Task AddNewProduct_New_Product_Created()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);

            var dto = new CreateProductDTO
            {
                Name = "Test Product",
                Price = 10,
                InStock = 50
            };


            var result = await controller.AddNewProduct(dto);


            Assert.IsType<OkObjectResult>(result);

            var product = context.Products.FirstOrDefault();

            Assert.NotNull(product);
            Assert.Equal("Test Product", product.Name);
            Assert.Equal(10, product.Price);
        }



        // ---------------------------------------- UpdateProduct ----------------------------------------
        [Fact]
        public async Task UpdateNewProduct_Product_NotFound()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);

            var dto = new UpdateProductDTO
            {
                Name = "New Name"
            };


            var result = await controller.UpdateProduct(1, dto);


            var resultNotFound = Assert.IsType<NotFoundObjectResult>(result);

            Assert.Equal("Product was not found", resultNotFound.Value);
        }


        [Fact]
        public async Task UpdateNewProduct_Product_No_Changes_BadRequest()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);

            var product = context.Products.Add(new Product
            {
                Id = 1,
                Name = "Name",
                Price = 10,
                InStock = 200,
            });

            var dto = new UpdateProductDTO { };


            var result = await controller.UpdateProduct(1, dto);


            var resultBadRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal("No changes were made", resultBadRequest.Value);
        }


        [Fact]
        public async Task UpdateNewProduct_Product_Updated()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);

            context.Products.Add(new Product
            {
                Id = 1,
                Name = "Name",
                Price = 10,
                InStock = 10,
            });
            context.SaveChanges();

            var dto = new UpdateProductDTO
            {
                Name = "New Name",
                Price = 120,
                InStock = 2
            };


            var result = await controller.UpdateProduct(1, dto);


            Assert.IsType<OkObjectResult>(result);

            var product = context.Products.FirstOrDefault();

            Assert.NotNull(product);
            Assert.Equal("New Name", product.Name);
            Assert.Equal(120, product.Price);
            Assert.Equal(2, product.InStock);
        }



        // ---------------------------------------- DeleteProduct ----------------------------------------
        [Fact]
        public async Task DeleteProduct_NotFound()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);


            var result = await controller.DeleteProduct(1);


            var resultNotFound = Assert.IsType<NotFoundObjectResult>(result);

            Assert.Equal("Product was not found", resultNotFound.Value);
        }


        [Fact]
        public async Task DeleteProduct_Ok()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);

            context.Products.Add(new Product
            {
                Id = 1,
                Name = "Name",
                Price = 10,
                InStock = 10,
            });
            context.SaveChanges();


            var result = await controller.DeleteProduct(1);


            var resultOk = Assert.IsType<OkObjectResult>(result);

            var dto = Assert.IsType<DeletedProductResponseDTO>(resultOk.Value);

            Assert.Equal("Product was successfully removed from the table", dto.Message);
        }



        // ---------------------------------------- GetAllUsers ----------------------------------------
        [Fact]
        public async Task GetAllUsers_Ok()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);

            context.Users.Add(new ApplicationUser
            {
                Id = "user1",
                UserName = "user",
                Email = "testUser@test.com",
                Balance = 100
            });
            context.SaveChanges();


            var result = await controller.GetAllUsers();


            Assert.IsType<OkObjectResult>(result);

            var user = context.Users.FirstOrDefault();

            Assert.Equal("user1", user.Id);
            Assert.Equal("testUser@test.com", user.Email);
            Assert.Equal(100, user.Balance);
        }



        // ---------------------------------------- GetOrdersInShoppingCart ----------------------------------------
        [Fact]
        public async Task GetOrdersInShoppingCart_Ok()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);

            var expectedOrder = new OrderedProduct
            {
                Id = 1,
                Products = "1",
                UserId = "1",
                TotalPrice = 10,
                Status = Statuses.InShoppingCart,
                OrderedAt = DateTime.UtcNow
            };
            context.OrderedProducts.Add(expectedOrder);

            var fakeOrder = new OrderedProduct
            {
                Id = 2,
                Products = "2",
                UserId = "2",
                TotalPrice = 20,
                Status = Statuses.Purchased,
                OrderedAt = DateTime.UtcNow
            };
            context.OrderedProducts.Add(fakeOrder);
            context.SaveChanges();


            var result = await controller.GetOrdersInShoppingCart();


            var resultOk = Assert.IsType<OkObjectResult>(result);

            var orders = Assert.IsType<List<OrderedProduct>>(resultOk.Value);

            Assert.Single(orders);

            var order = orders[0];
            Assert.Equal(1, order.Id);
            Assert.Equal(Statuses.InShoppingCart, order.Status);
            Assert.Equal(10, order.TotalPrice);
        }



        // ---------------------------------------- GetOrdersPurchased ----------------------------------------
        [Fact]
        public async Task GetOrdersPurchased_Ok()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);

            var fakeOrder = new OrderedProduct
            {
                Id = 1,
                Products = "1",
                UserId = "1",
                TotalPrice = 10,
                Status = Statuses.InShoppingCart,
                OrderedAt = DateTime.UtcNow
            };
            context.OrderedProducts.Add(fakeOrder);

            var expectedOrder = new OrderedProduct
            {
                Id = 2,
                Products = "2",
                UserId = "2",
                TotalPrice = 20,
                Status = Statuses.Purchased,
                OrderedAt = DateTime.UtcNow
            };
            context.OrderedProducts.Add(expectedOrder);
            context.SaveChanges();


            var result = await controller.GetOrdersPurchased();


            var resultOk = Assert.IsType<OkObjectResult>(result);

            var orders = Assert.IsType<List<OrderedProduct>>(resultOk.Value);

            Assert.Single(orders);

            var order = orders[0];
            Assert.Equal(2, order.Id);
            Assert.Equal(Statuses.Purchased, order.Status);
            Assert.Equal(20, order.TotalPrice);
        }



        // ---------------------------------------- ChangeOrderStatus ----------------------------------------
        [Fact]
        public async Task ChangeOrderStatus_NotFound()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;


            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);

            context.OrderedProducts.Add(new OrderedProduct
            {
                Id = 1,
                Products = "1",
                UserId = "1",
                TotalPrice = 100,
                Status = Statuses.InShoppingCart,
                OrderedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var dto = new ChangeOrderStatusDTO
            {
                OrderId = 999,
                Status = Statuses.Purchased
            };


            var result = await controller.ChangeOrderStatus(dto);


            var resultNotFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"Order ID: {dto.OrderId} was not found", resultNotFound.Value);
        }


        [Fact]
        public async Task ChangeOrderStatus_BadRequest_Purchased_State()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;


            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);

            context.OrderedProducts.Add(new OrderedProduct
            {
                Id = 1,
                Products = "1",
                UserId = "1",
                TotalPrice = 100,
                Status = Statuses.Purchased,
                OrderedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var dto = new ChangeOrderStatusDTO
            {
                OrderId = 1,
                Status = Statuses.InShoppingCart
            };


            var result = await controller.ChangeOrderStatus(dto);


            var resultBadRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal($"Order was already purchased", resultBadRequest.Value);
        }


        [Fact]
        public async Task ChangeOrderStatus_BadRequest_Cancelled_State()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;


            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);

            context.OrderedProducts.Add(new OrderedProduct
            {
                Id = 1,
                Products = "1",
                UserId = "1",
                TotalPrice = 100,
                Status = Statuses.Cancelled,
                OrderedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var dto = new ChangeOrderStatusDTO
            {
                OrderId = 1,
                Status = Statuses.InShoppingCart
            };


            var result = await controller.ChangeOrderStatus(dto);


            var resultBadRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal($"Cancelled order cannot be changed", resultBadRequest.Value);
        }


        [Fact]
        public async Task ChangeOrderStatus_BadRequest_Same_State()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;


            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);

            context.OrderedProducts.Add(new OrderedProduct
            {
                Id = 1,
                Products = "1",
                UserId = "1",
                TotalPrice = 100,
                Status = Statuses.InShoppingCart,
                OrderedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var dto = new ChangeOrderStatusDTO
            {
                OrderId = 1,
                Status = Statuses.InShoppingCart
            };


            var result = await controller.ChangeOrderStatus(dto);


            var resultBadRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal($"Order ID: {dto.OrderId} already had this status: {dto.Status}", resultBadRequest.Value);
        }


        [Fact]
        public async Task ChangeOrderStatus_NotFound_User()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;


            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);

            context.Users.Add(new ApplicationUser
            {
                Id = "user1",
                UserName = "user",
                Email = "user@test.com",
                Balance = 100
            });

            context.OrderedProducts.Add(new OrderedProduct
            {
                Id = 1,
                Products = "1",
                UserId = "1",
                TotalPrice = 100,
                Status = Statuses.InShoppingCart,
                OrderedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var dto = new ChangeOrderStatusDTO
            {
                OrderId = 1,
                Status = Statuses.Purchased
            };


            var result = await controller.ChangeOrderStatus(dto);


            var resultNotFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"User of the order {dto.OrderId} was not found", resultNotFound.Value);
        }


        [Fact]
        public async Task ChangeOrderStatus_BadRequest_Balance()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;


            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);

            context.Users.Add(new ApplicationUser
            {
                Id = "user1",
                UserName = "user",
                Email = "user@test.com",
                Balance = 10
            });

            context.OrderedProducts.Add(new OrderedProduct
            {
                Id = 1,
                Products = "1",
                UserId = "user1",
                TotalPrice = 100,
                Status = Statuses.InShoppingCart,
                OrderedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var dto = new ChangeOrderStatusDTO
            {
                OrderId = 1,
                Status = Statuses.Purchased
            };


            var result = await controller.ChangeOrderStatus(dto);


            var resultBadRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal($"User does not have enough money for this order ID: {dto.OrderId}", resultBadRequest.Value);
        }


        [Fact]
        public async Task ChangeOrderStatus_BadRequest_No_Products()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;


            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);

            context.Users.Add(new ApplicationUser
            {
                Id = "user1",
                UserName = "user",
                Email = "user@test.com",
                Balance = 1000
            });

            context.OrderedProducts.Add(new OrderedProduct
            {
                Id = 1,
                Products = "",
                UserId = "user1",
                TotalPrice = 100,
                Status = Statuses.InShoppingCart,
                OrderedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var dto = new ChangeOrderStatusDTO
            {
                OrderId = 1,
                Status = Statuses.Purchased
            };


            var result = await controller.ChangeOrderStatus(dto);


            var resultBadRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No products", resultBadRequest.Value);
        }


        [Fact]
        public async Task ChangeOrderStatus_BadRequest_Out_Of_Stock()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;


            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);

            context.Users.Add(new ApplicationUser
            {
                Id = "user1",
                UserName = "user",
                Email = "user@test.com",
                Balance = 1000
            });

            context.Products.Add(new Product
            {
                Id = 1,
                Name = "Product",
                Price = 100,
                InStock = 0,
                IsAvailable = false
            });

            context.OrderedProducts.Add(new OrderedProduct
            {
                Id = 1,
                Products = "[1]",
                UserId = "user1",
                TotalPrice = 100,
                Status = Statuses.InShoppingCart,
                OrderedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var dto = new ChangeOrderStatusDTO
            {
                OrderId = 1,
                Status = Statuses.Purchased
            };


            var result = await controller.ChangeOrderStatus(dto);


            var product = context.Products.FirstOrDefault();

            var resultBadRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal($"Product {product!.Name} is out of stock", resultBadRequest.Value);
        }


        [Fact]
        public async Task ChangeOrderStatus_BadRequest_Is_Available()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;


            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);

            context.Users.Add(new ApplicationUser
            {
                Id = "user1",
                UserName = "user",
                Email = "user@test.com",
                Balance = 1000
            });

            context.Products.Add(new Product
            {
                Id = 1,
                Name = "Product",
                Price = 100,
                InStock = 10,
                IsAvailable = false
            });

            context.OrderedProducts.Add(new OrderedProduct
            {
                Id = 1,
                Products = "[1]",
                UserId = "user1",
                TotalPrice = 100,
                Status = Statuses.InShoppingCart,
                OrderedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var dto = new ChangeOrderStatusDTO
            {
                OrderId = 1,
                Status = Statuses.Purchased
            };


            var result = await controller.ChangeOrderStatus(dto);


            var product = context.Products.FirstOrDefault();

            var resultBadRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal($"Product is currently not available", resultBadRequest.Value);
        }

        [Fact]
        public async Task ChangeOrderStatus_BadRequest_Products_Ids_Are_String()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;


            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);

            context.Users.Add(new ApplicationUser
            {
                Id = "user1",
                UserName = "user",
                Email = "user@test.com",
                Balance = 1000
            });

            context.Products.Add(new Product
            {
                Id = 1,
                Name = "Product",
                Price = 100,
                InStock = 10,
                IsAvailable = true
            });

            context.OrderedProducts.Add(new OrderedProduct
            {
                Id = 1,
                Products = "1",
                UserId = "user1",
                TotalPrice = 100,
                Status = Statuses.InShoppingCart,
                OrderedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var dto = new ChangeOrderStatusDTO
            {
                OrderId = 1,
                Status = Statuses.Purchased
            };


            var result = await controller.ChangeOrderStatus(dto);


            var product = context.Products.FirstOrDefault();

            var resultBadRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal($"Invalid input. Products' ids must be a list", resultBadRequest.Value);
        }


        [Fact]
        public async Task ChangeOrderStatus_Ok()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;


            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);

            context.Users.Add(new ApplicationUser
            {
                Id = "user1",
                UserName = "user",
                Email = "user@test.com",
                Balance = 1000
            });

            context.Products.Add(new Product
            {
                Id = 1,
                Name = "Product",
                Price = 100,
                InStock = 10,
                IsAvailable = true
            });

            context.OrderedProducts.Add(new OrderedProduct
            {
                Id = 1,
                Products = "[1]",
                UserId = "user1",
                TotalPrice = 100,
                Status = Statuses.InShoppingCart,
                OrderedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var dto = new ChangeOrderStatusDTO
            {
                OrderId = 1,
                Status = Statuses.Purchased
            };


            var result = await controller.ChangeOrderStatus(dto);


            var product = context.Products.FirstOrDefault();

            Assert.IsType<OkObjectResult>(result);
        }


        [Fact]
        public async Task ChangeOrderStatus_Server_Error()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;


            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            logServiceMock.Setup(service => service.Add(It.IsAny<LogFile>()))
                .Throws(new Exception("Database connection failed during logging"));

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);

            context.Users.Add(new ApplicationUser
            {
                Id = "user1",
                UserName = "user",
                Email = "user@test.com",
                Balance = 1000
            });

            context.Products.Add(new Product
            {
                Id = 1,
                Name = "Product",
                Price = 100,
                InStock = 10,
                IsAvailable = true
            });

            context.OrderedProducts.Add(new OrderedProduct
            {
                Id = 1,
                Products = "[1]",
                UserId = "user1",
                TotalPrice = 100,
                Status = Statuses.InShoppingCart,
                OrderedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var dto = new ChangeOrderStatusDTO
            {
                OrderId = 1,
                Status = Statuses.Purchased
            };


            var result = await controller.ChangeOrderStatus(dto);


            var resultError = Assert.IsType<ObjectResult>(result);

            Assert.Equal(500, resultError.StatusCode);

            Assert.Equal("Server error Database connection failed during logging", resultError.Value);
        }



        // ---------------------------------------- ChangeAccountBalance ----------------------------------------
        [Fact]
        public async Task ChangeAccountBalance_NotFound_User()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;


            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);

            var dto = new ChangeBalanceDTO
            {
                AccountId = "1",
                NewBalance = 10
            };


            var result = await controller.ChangeAccountBalance(dto);


            var resultNotFound = Assert.IsType<NotFoundObjectResult>(result);

            Assert.Equal("User not found", resultNotFound.Value);
        }


        [Fact]
        public async Task ChangeAccountBalance_BadRequest_Balance()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            var testUser = new ApplicationUser
            {
                Id = "user1",
                UserName = "user",
                Email = "user@test.com",
                Balance = 1000
            };

            userManagerMock.Setup(u => u.FindByIdAsync("user1"))
                           .ReturnsAsync(testUser);

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);

            var dto = new ChangeBalanceDTO
            {
                AccountId = "user1",
                NewBalance = -1
            };


            var result = await controller.ChangeAccountBalance(dto);


            var resultBadRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Balance cannot be negative", resultBadRequest.Value);
        }


        [Fact]
        public async Task ChangeAccountBalance_Ok()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var context = new AppDbContext(options);
            var userManagerMock = MockUserManager();
            var logServiceMock = new Mock<LogService>();

            var testUser = new ApplicationUser
            {
                Id = "user1",
                UserName = "user",
                Email = "user@test.com",
                Balance = 1000
            };

            var user = userManagerMock.Setup(u => u.FindByIdAsync("user1"))
                           .ReturnsAsync(testUser);

            var controller = new AdminController(context, userManagerMock.Object, logServiceMock.Object);

            var dto = new ChangeBalanceDTO
            {
                AccountId = "user1",
                NewBalance = 10
            };


            var result = await controller.ChangeAccountBalance(dto);


            var resultOk = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ChangeAccountBalanceResponseDTO>(resultOk.Value);

            Assert.Equal("Balance updated", response.Message);
            Assert.Equal(10, response.Balance);
        }
    }        
}