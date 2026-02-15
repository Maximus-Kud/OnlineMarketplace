using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OnlineMarketplace.DTO;
using OnlineMarketplace.Models;
using OnlineMarketplace.Roles;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnlineMarketplace.Controllers
{
    [ApiController]
    [Route("Authentication")]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly LogService _logService;


        public AuthenticationController(UserManager<ApplicationUser> userManager, IConfiguration configuration, LogService logService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _logService = logService;
        }
        
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegistrationDTO registrationData)
        {
            if (!ModelState.IsValid) return BadRequest("Invalid data " + ModelState);


            var user = new ApplicationUser
            {
                UserName = registrationData.Username,
                Email = registrationData.Email
            };


            var userFull = await _userManager.CreateAsync(user, registrationData.Password);
            if (!userFull.Succeeded) return BadRequest(userFull.Errors);

            if (registrationData.Email == "owner@email.com")
                await _userManager.AddToRoleAsync(user, RoleNames.Owner);
            else if (registrationData.Email == "admin@email.com")
                await _userManager.AddToRoleAsync(user, RoleNames.Admin);
            else
                await _userManager.AddToRoleAsync(user, RoleNames.Customer);

            var roles = await _userManager.GetRolesAsync(user);

            _logService.Add(new LogFile
            {
                Date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                LogLevel = LogFile.LogLevels.INFO,
                User = await _userManager.GetUserAsync(User) ?? new ApplicationUser { UserName = registrationData.Username, Email = registrationData.Email },
                Role = roles.FirstOrDefault() ?? RoleNames.Customer,
                Description = "Created POST-Request to register"
            });


            return Ok(new
            {
                Message = "User created",
                Username = registrationData.Username,
                Email = registrationData.Email
            });
        }


        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDTO loginData)
        {
            if (!ModelState.IsValid) return BadRequest("Invalid data " + ModelState);


            var user = await _userManager.FindByNameAsync(loginData.Username);
            if (user == null) return NotFound("User was not found");


            var validPassword = await _userManager.CheckPasswordAsync(user, loginData.Password);
            if (!validPassword) return NotFound("User with this password was not found");


            var roles = await _userManager.GetRolesAsync(user);


            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            _logService.Add(new LogFile
            {
                Date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                LogLevel = LogFile.LogLevels.INFO,
                User = await _userManager.GetUserAsync(User) ?? new ApplicationUser { Id = user.Id, UserName = user.UserName, Email = user.Email },
                Role = roles.FirstOrDefault() ?? RoleNames.Customer,
                Description = "Created POST-Request to log and get JWT token"
            });


            return Ok(new
            {
                User = user.UserName,
                Email = user.Email,
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }




        [HttpGet("test")]
        [Authorize]
        public async Task<ActionResult> Test()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            _logService.Add(new LogFile
            {
                Date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                LogLevel = LogFile.LogLevels.DEBUG,
                User = await _userManager.GetUserAsync(User) ?? new ApplicationUser { },
                Role = User.FindFirstValue(ClaimTypes.Role) ?? RoleNames.Customer,
                Description = "Created GET-Request to test"
            });


            return Ok(new
            {
                UserName = User.Identity?.Name,
                IsAuthenticated = User.Identity?.IsAuthenticated,
                AllClaims = claims,
                DetectedRoles = roles
            });
        }
    }
}
