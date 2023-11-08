using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using GeekShopping.ProductAPI.Model.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GeekShopping.ProductAPI.Controllers.Auth
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public AuthController(IConfiguration configuration, IUserService userService)
        {
            _configuration = configuration;
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserCredentials credentials)
        {
            bool result = _userService.RegisterUser(credentials);
            if (!result) return BadRequest("User could not be created");

            return StatusCode(201);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserCredentials credentials)
        {
            var user = _userService.ValidateUser(credentials);
            if (user == null) return Unauthorized();
            
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Name),
            };
            
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var tokenOptions = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: signinCredentials
            );
            
            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            return Ok(new { Token = tokenString });
        }
    }

    public class UserCredentials
    {
        public string Name { get; set; } 
        public string Password { get; set; }
    }

    public interface IUserService
    {
        bool RegisterUser(UserCredentials userCredentials);
        UserCredentials ValidateUser(UserCredentials userCredentials);
    }

    public class UserService : IUserService
    {
        private readonly MySQLContext _context;

        public UserService(MySQLContext context)
        {
            _context = context;
        }

        public bool RegisterUser(UserCredentials userCredentials)
        {
            var newUser = new User
            {
                UserName = userCredentials.Name,
                Password = HashPassword(userCredentials.Password)
            };
            _context.Users.Add(newUser);
            _context.SaveChanges();

            return true;
        }

        public UserCredentials ValidateUser(UserCredentials userCredentials)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == userCredentials.Name);

            if (user != null && BCrypt.Net.BCrypt.Verify(userCredentials.Password, user.Password))
            {
                return new UserCredentials { Name = user.UserName };
            }

            return null;
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}