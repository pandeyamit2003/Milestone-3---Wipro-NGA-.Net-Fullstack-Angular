using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SecureNoteAPI.Data;
using SecureNoteAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SecureNoteAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public IActionResult Register(User user)
        {
            if (_context.Users.Any(x =>
                x.Username == user.Username))
            {
                return BadRequest(
                    "Username already exists");
            }

            user.Password =
                BCrypt.Net.BCrypt.HashPassword(
                    user.Password);

            _context.Users.Add(user);

            _context.SaveChanges();

            return Ok(new
            {
                message =
                "User registered successfully. Please log in."
            });
        }
        [HttpPost("login")]
        public IActionResult Login(LoginModel login)
        {
            var user = _context.Users
                .FirstOrDefault(x => x.Username == login.Username);

            if (user == null)
                return Unauthorized();

            bool valid = BCrypt.Net.BCrypt.Verify(
                login.Password,
                user.Password);

            if (!valid)
            {
                return Unauthorized(new
                {
                    message = "Invalid Password"
                });
            }

            var claims = new[]
            {
        new Claim(ClaimTypes.Name, user.Username)
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler()
                    .WriteToken(token),

                expires_in = 3600,

                user = new
                {
                    username = user.Username
                }
            });
        }
    }
        }
    
