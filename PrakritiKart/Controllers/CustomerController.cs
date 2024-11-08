using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrakritiKart.Interfaces;
using PrakritiKart.Models;

namespace PrakritiKart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {

        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        //############################# Address Controller ############################################
        [Authorize]
        [HttpGet("address/all")]

        public async Task<IActionResult> GetUserAddresses()
        {
            // Retrieve the userId from the JWT claims

            var customerIdClaim = User.FindFirst("customerid")?.Value;

            if (customerIdClaim == null)
            {
                return Unauthorized("Customer ID is not available in the token.");
            }

            // Directly convert userIdClaim to int
            int customerId = Convert.ToInt32(customerIdClaim);


            // Pass the extracted userId to the service
            var addresses = await _customerService.GetCustomerAddressesAsync(customerId);

            // Return the addresses as a response
            return Ok(addresses);
        }


        [HttpPost("address/add")]

        public async Task<IActionResult> AddAddress([FromBody] Address address)
        {
            var customerIdClaim = User.FindFirst("customerid")?.Value;

            if (customerIdClaim == null)
            {
                return Unauthorized("Customer ID is not available in the token.");
            }
            int customerId = Convert.ToInt32(customerIdClaim);

            var result = await _customerService.AddAddressAsync(customerId, address);

            return Ok(result);
        }

        [Authorize]
        [HttpPut("address/edit/{addressId}")]
        public async Task<IActionResult> EditAddress(int addressId, [FromBody] Address address)
        {
            var customerIdClaim = User.FindFirst("customerid")?.Value;
            if (customerIdClaim == null)
            {
                return Unauthorized("Customer ID is not available in the token.");
            }
            int customerId = Convert.ToInt32(customerIdClaim);

            var result = await _customerService.UpdateAddressAsync(customerId, addressId, address);
            if (result == null)
            {
                return NotFound("Address not found or access is unauthorized.");
            }
            return Ok(result);
        }


        [HttpDelete("address/delete/{addressId}")]
        public async Task<IActionResult> DeleteAddress(int addressId)
        {
            var customerIdClaim = User.FindFirst("customerid")?.Value;
            if (customerIdClaim == null)
            {
                return Unauthorized("Customer ID is not available in the token.");
            }

            if (!int.TryParse(customerIdClaim, out int customerId))
            {
                return BadRequest("Invalid Customer ID.");
            }

            var success = await _customerService.DeleteAddressAsync(customerId, addressId);
            if (!success)
            {
                return NotFound("Address not found or access is unauthorized.");
            }

            return NoContent(); // Consider returning NoContent for successful deletion
        }
    }
}
