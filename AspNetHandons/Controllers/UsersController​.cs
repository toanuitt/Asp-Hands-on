using AspNetHandons.Entities;
using AspNetHandons.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AspNetHandons.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly ILogger<UserController> _logger;

        private static List<User> users = new()
        {
            new User
            {
                Name = "test",
                Email = "test@email.com",
                Password = "123",
                Role = "Admin",
                Permissions = new List<string>
                {
                    "orders.read",
                    "orders.write",
                }
            },
            new User
            {
                Name = "user",
                Email = "user@email.com",
                Password = "123",
                Role = "User",
                Permissions = new List<string>
                {
                    "orders.read"
                }
            }
        };

        public UserController(JwtService jwtService,
                              ILogger<UserController> logger)
        {
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User request)
        {
            _logger.LogInformation("Login attempt for user: {Username}", request.Name);

            var user = users.FirstOrDefault(u =>
                u.Name == request.Name &&
                u.Password == request.Password);

            if (user == null)
            {
                _logger.LogWarning("Login failed for user: {Username}", request.Name);
                return Unauthorized("Invalid username or password");
            }

            var token = _jwtService.GenerateJwt(user);

            _logger.LogInformation("Login successful for user: {Username}", user.Name);

            return Ok(new
            {
                message = "Login successful",
                token
            });
        }

        [Authorize]
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var username = User.Identity?.Name;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            _logger.LogInformation("Profile accessed by {Username}", username);

            return Ok(new
            {
                message = "Authorized successfully",
                username,
                email,
                role
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult AdminOnly()
        {
            _logger.LogInformation("Admin endpoint accessed by {User}", User.Identity?.Name);
            return Ok("Welcome Admin");
        }

        [Authorize(Roles = "User")]
        [HttpGet("user")]
        public IActionResult UserOnly()
        {
            _logger.LogInformation("User endpoint accessed by {User}", User.Identity?.Name);
            return Ok("Welcome User");
        }
    }
}