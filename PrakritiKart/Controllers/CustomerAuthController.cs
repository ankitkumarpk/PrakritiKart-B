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
    public class CustomerAuthController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IConfiguration _configuration;

        public CustomerAuthController(ICustomerService customerService, IConfiguration configuration)
        {
            _customerService = customerService;
            _configuration = configuration;
        }

        // Registration
        [HttpPost("signup")]
        public async Task<IActionResult> Register([FromBody] SignUpDto signupDto)
        {
            // Check if customer already exists by email
            var existingCustomer = await _customerService.GetCustomerByEmailAsync(signupDto.Email);
            if (existingCustomer != null)
            {
                return BadRequest("Email is already in use.");
            }

            // Generate a salt
            var salt = GenerateSalt();

            // Hash the password using SHA-512 with the generated salt
            var hashedPassword = HashPasswordWithSHA512(signupDto.Password, salt);

            // Create a new Customer object
            var customer = new Customer
            {
                FirstName = signupDto.FirstName,
                LastName = signupDto.LastName,
                Email = signupDto.Email,
                PasswordHash = hashedPassword,
                Salt = salt,
                PhoneNumber = signupDto.PhoneNumber,
                EmailVerified = false,
                UserType = signupDto.UserType // Set customer type during registration
            };

            var customerId = await _customerService.RegisterCustomerAsync(customer);
            if (customerId > 0)
            {
                // Generate a token after successful registration
                var token = GenerateJwtToken(customer);
                return Ok(new { Token = token });
            }

            return StatusCode(500, "Error registering customer.");
        }

        // Login functionality
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var customer = await _customerService.GetCustomerByEmailAsync(loginDto.Email);
            if (customer == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            var enteredPasswordHash = HashPasswordWithSHA512(loginDto.Password, customer.Salt);
            if (enteredPasswordHash != customer.PasswordHash)
            {
                return Unauthorized("Invalid email or password.");
            }

            if (customer.UserType != loginDto.UserType)
            {
                return BadRequest("Not a valid customer type");

            }

            // Generate JWT token upon successful login
            var token = GenerateJwtToken(customer);
            return Ok(new
            {
                Token = token,
                FirstName = customer.FirstName,
                UserType = customer.UserType         // Include the customer type in the response
            }
            );
        }

        // Method to generate a JWT token with role-based claim
        private string GenerateJwtToken(Customer customer)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, customer.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("customerid", customer.CustomerId.ToString()),
                    new Claim("email", customer.Email),
                    new Claim(ClaimTypes.Role, customer.UserType)    // Add customer type as a role claim
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
