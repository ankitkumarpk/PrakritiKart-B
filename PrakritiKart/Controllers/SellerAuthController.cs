using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PrakritiKart.Interfaces;
using PrakritiKart.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PrakritiKart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SellerAuthController : ControllerBase
    {


        private readonly ISellerService _sellerService;
        private readonly IConfiguration _configuration;

        public SellerAuthController(ISellerService sellerService, IConfiguration configuration)
        {
            _sellerService = sellerService;
            _configuration = configuration;
        }

        // Registration
        [HttpPost("signup")]
        public async Task<IActionResult> Register([FromBody] SignUpDto signupDto)
        {
            // Check if seller already exists by email
            var existingSeller = await _sellerService.GetSellerByEmailAsync(signupDto.Email);
            if (existingSeller != null)
            {
                return BadRequest("Email is already in use.");
            }

            // Generate a salt
            var salt = GenerateSalt();

            // Hash the password using SHA-512 with the generated salt
            var hashedPassword = HashPasswordWithSHA512(signupDto.Password, salt);

            // Create a new Seller object
            var seller = new Seller
            {
                FirstName = signupDto.FirstName,
                LastName = signupDto.LastName,
                Email = signupDto.Email,
                PasswordHash = hashedPassword,
                Salt = salt,
                PhoneNumber = signupDto.PhoneNumber,
                EmailVerified = false,
                UserType = signupDto.UserType // Set seller type during registration
            };

            var sellerId = await _sellerService.RegisterSellerAsync(seller);
            if (sellerId > 0)
            {
                // Generate a token after successful registration
                var token = GenerateJwtToken(seller);
                return Ok(new { Token = token });
            }

            return StatusCode(500, "Error registering seller.");
        }

        // Login functionality
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var seller = await _sellerService.GetSellerByEmailAsync(loginDto.Email);
            if (seller == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            var enteredPasswordHash = HashPasswordWithSHA512(loginDto.Password, seller.Salt);
            if (enteredPasswordHash != seller.PasswordHash)
            {
                return Unauthorized("Invalid email or password.");
            }

            if (seller.UserType != loginDto.UserType)
            {
                return BadRequest("Not a valid user type");

            }

            // Generate JWT token upon successful login
            var token = GenerateJwtToken(seller);
            return Ok(new
            {
                Token = token,
                FirstName = seller.FirstName,
                UserType = seller.UserType         // Include the seller type in the response
            }
            );
        }

        // Method to generate a JWT token with role-based claim
        private string GenerateJwtToken(Seller seller)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, seller.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("sellerid", seller.SellerId.ToString()),
                    new Claim("email", seller.Email),
                    new Claim(ClaimTypes.Role, seller.UserType)    // Add seller type as a role claim
                }),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiryInMinutes"])),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // Method to hash password with SHA512 and a salt
        private static string HashPasswordWithSHA512(string password, string salt)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] saltedPasswordBytes = Encoding.UTF8.GetBytes(password + salt);
                byte[] hashedBytes = sha512.ComputeHash(saltedPasswordBytes);

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashedBytes.Length; i++)
                {
                    builder.Append(hashedBytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private static string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

    }
}
